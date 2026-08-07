using System;
using System.Collections.Generic;
using LogiCard.Sim;

namespace LogiCard.Net
{
    /// <summary>
    /// One pawn's contribution to a resolve. The payload has no identity or origin of its own
    /// (Day 11's RPC carries the sender separately), so the resolver is told both here.
    /// </summary>
    public readonly struct GhostInput
    {
        public int PawnId { get; }

        public PlanarPosition Start { get; }

        public TimelinePayload Payload { get; }

        /// <summary>Wounds carried in from prior rounds (C33). Zero on the first round.</summary>
        public int StartingWounds { get; }

        public GhostInput(int pawnId, PlanarPosition start, TimelinePayload payload, int startingWounds = 0)
        {
            PawnId = pawnId;
            Start = start;
            Payload = payload;
            StartingWounds = startingWounds < 0 ? 0 : startingWounds;
        }
    }

    /// <summary>
    /// Turns locked <see cref="TimelinePayload"/>s into a <see cref="ReplayTape"/> (C23): the offline
    /// stand-in for the Fusion Host's authoritative ghost sim.
    ///
    /// Resolve is a pure function of (board, inputs) — no UnityEngine.Time, no Random, no physics.
    ///
    /// Retargeted onto continuous space (C35/C39 pivot) — this is the same event-sweep /
    /// simultaneity-epsilon / group-then-apply / scratch-board-clone shape as the Day 7 door
    /// machinery, just pointed at <see cref="ArenaBoard"/>. The one genuinely new algorithm is
    /// <see cref="TryFindHoldContact"/>'s analytic sweep (Decision 5): the old grid version sampled a
    /// handful of discrete instants and got away with it only because each waypoint was exactly one
    /// tile, so sampling was densely spaced by coincidence. A continuous leg can be one long straight
    /// line between two waypoints, so a victim can sweep through the lane strictly between two samples
    /// undetected — this needs a closed-form check instead.
    ///
    /// Combat (Days 4–6):
    /// - <see cref="ActionNode.ExecuteTime"/> is a *completion* second.
    /// - Snap Shot: within <see cref="HitRadius"/> of the aim point at completion only (C32); needs LoS to aim and victim; wounds; misses Sprint.
    /// - Hold Angle: covers the aim lane (within <see cref="LaneHalfWidth"/>) across its window; lethal; hits Sprint.
    /// - Simultaneous shots in a group are judged before wounds apply (mutual exchange / mutual lethal).
    /// </summary>
    public sealed class GhostResolver
    {
        public const int WoundsUntilDead = 2;

        /// <summary>
        /// Free-aim hit/lane tolerance (Decision 1/5) — start ~half a pawn-width per C39, tune in
        /// Phase 6 of CONTINUOUS_PIVOT_PLAN.md against real play (PRODUCT_MEMORY.md open numeric #5).
        /// </summary>
        private const float HitRadius = 0.45f;

        private const float LaneHalfWidth = 0.45f;

        private const float DefaultSimultaneityEpsilon = 0.01f;
        private static readonly IReadOnlyList<ActionNode> NoNodes = new List<ActionNode>();

        private readonly ArenaBoard _board;
        private readonly float _simultaneityEpsilon;

        public GhostResolver(ArenaBoard board, float simultaneityEpsilon = DefaultSimultaneityEpsilon)
        {
            _board = board;
            _simultaneityEpsilon = simultaneityEpsilon;
        }

        public ReplayTape Resolve(IReadOnlyList<GhostInput> inputs)
        {
            var tracks = new Dictionary<int, GhostTrack>();
            var order = new List<int>();
            var events = new List<TapeEvent>();

            if (inputs != null)
            {
                for (int i = 0; i < inputs.Count; i++)
                {
                    GhostInput input = inputs[i];
                    tracks[input.PawnId] = CompileTrack(input, events);
                    order.Add(input.PawnId);
                }
            }

            order.Sort();
            ResolveShots(tracks, order, events);

            events.Sort(CompareEvents);

            var paths = new Dictionary<int, ScheduledPath>();
            var endWounds = new Dictionary<int, int>();
            for (int i = 0; i < order.Count; i++)
            {
                int pawnId = order[i];
                paths[pawnId] = tracks[pawnId].Path;
                endWounds[pawnId] = tracks[pawnId].Wounds;
            }

            return new ReplayTape(paths, events, endWounds);
        }

        private static GhostTrack CompileTrack(GhostInput input, List<TapeEvent> events)
        {
            var ordered = new List<ActionNode>(input.Payload != null ? input.Payload.Nodes : NoNodes);
            ordered.Sort((a, b) => a.ExecuteTime.CompareTo(b.ExecuteTime));

            var waypoints = new List<PlanarPosition> { input.Start };
            var arrivals = new List<float> { 0f };
            PlanarPosition current = input.Start;

            foreach (ActionNode node in ordered)
            {
                if (node.Verb == ActionVerb.Move)
                {
                    current = node.Position;
                    events.Add(new TapeEvent(node.ExecuteTime, input.PawnId, TapeEventType.MoveArrive, current));
                }

                waypoints.Add(current);
                arrivals.Add(node.ExecuteTime);
            }

            return new GhostTrack(ScheduledPath.FromTimedWaypoints(waypoints, arrivals), ordered, input.StartingWounds);
        }

        private void ResolveShots(Dictionary<int, GhostTrack> tracks, List<int> order, List<TapeEvent> events)
        {
            var shots = new List<ShotIntent>();
            var doorToggles = new List<DoorToggleIntent>();
            for (int i = 0; i < order.Count; i++)
            {
                GhostTrack track = tracks[order[i]];
                foreach (ActionNode node in track.Nodes)
                {
                    if (node.Verb == ActionVerb.Shoot)
                    {
                        ShootMode mode = node.ShootMode == ShootMode.None ? ShootMode.SnapShot : node.ShootMode;
                        float cost = ShootCost.SecondsFor(mode);
                        float windowStart = node.ExecuteTime - cost;
                        if (windowStart < 0f)
                        {
                            windowStart = 0f;
                        }

                        shots.Add(new ShotIntent(order[i], node.ExecuteTime, windowStart, node.Position, mode));
                    }
                    else if (node.Verb == ActionVerb.Door)
                    {
                        // Position is the door's interaction point, written by whatever queued the
                        // node (PawnProgram in Phase 3) using the same value ArenaBoard registered it
                        // with, so this exact-match lookup resolves back to the right Door instance.
                        if (_board.TryGetDoor(node.Position, out Door door))
                        {
                            doorToggles.Add(new DoorToggleIntent(order[i], node.ExecuteTime, door, node.Position, node.Door));
                        }
                    }
                }
            }

            // Sort by resolve instant: Snap at completion; Hold also stamps consequences at first
            // contact, but the fire event is still listed at completion for scrubber readability.
            shots.Sort((a, b) => a.CompleteSeconds != b.CompleteSeconds
                ? a.CompleteSeconds.CompareTo(b.CompleteSeconds)
                : a.ShooterId.CompareTo(b.ShooterId));

            // Door toggles land before same-instant shots resolve (Day 7 research note §H5): a
            // door closing at the exact second a shot completes blocks that shot.
            doorToggles.Sort((a, b) => a.ExecuteTime != b.ExecuteTime
                ? a.ExecuteTime.CompareTo(b.ExecuteTime)
                : a.PawnId.CompareTo(b.PawnId));

            // Resolve-local scratch board (Day 7, Option 1): doors mutate this copy in ExecuteTime
            // order as the sweep below reaches them, never the shared _board instance.
            ArenaBoard scratch = _board.Clone();

            var hits = new List<ResolvedHit>();
            int shotIndex = 0;
            int doorIndex = 0;
            while (shotIndex < shots.Count || doorIndex < doorToggles.Count)
            {
                bool doorNext = doorIndex < doorToggles.Count
                    && (shotIndex >= shots.Count || doorToggles[doorIndex].ExecuteTime <= shots[shotIndex].CompleteSeconds);

                if (doorNext)
                {
                    int doorGroupEnd = doorIndex + 1;
                    while (doorGroupEnd < doorToggles.Count
                           && ReferenceEquals(doorToggles[doorGroupEnd].Door, doorToggles[doorIndex].Door)
                           && doorToggles[doorGroupEnd].ExecuteTime - doorToggles[doorIndex].ExecuteTime <= _simultaneityEpsilon)
                    {
                        doorGroupEnd++;
                    }

                    ApplyDoorGroup(doorToggles, doorIndex, doorGroupEnd, scratch, events);
                    doorIndex = doorGroupEnd;
                    continue;
                }

                int shotGroupEnd = shotIndex + 1;
                while (shotGroupEnd < shots.Count
                       && shots[shotGroupEnd].CompleteSeconds - shots[shotIndex].CompleteSeconds <= _simultaneityEpsilon)
                {
                    shotGroupEnd++;
                }

                hits.Clear();
                for (int i = shotIndex; i < shotGroupEnd; i++)
                {
                    ResolveShot(shots[i], tracks, order, events, hits, scratch);
                }

                for (int i = 0; i < hits.Count; i++)
                {
                    ApplyHit(hits[i], tracks, events);
                }

                shotIndex = shotGroupEnd;
            }
        }

        /// <summary>
        /// Applies every toggle in a same-instant, same-door group to <paramref name="scratch"/> and
        /// logs one tape event per attempt. Close wins over Open within the group (Day 7 research
        /// note §H5) — a simultaneous Open/Close on the same door ends up Closed.
        /// </summary>
        private static void ApplyDoorGroup(
            List<DoorToggleIntent> toggles, int start, int end, ArenaBoard scratch, List<TapeEvent> events)
        {
            bool anyClose = false;
            for (int i = start; i < end; i++)
            {
                if (toggles[i].Action == DoorAction.Close)
                {
                    anyClose = true;
                    break;
                }
            }

            DoorAction resultingAction = anyClose ? DoorAction.Close : DoorAction.Open;
            scratch.SetDoorState(toggles[start].Door, resultingAction == DoorAction.Open ? DoorState.Open : DoorState.Closed);

            for (int i = start; i < end; i++)
            {
                TapeEventType type = toggles[i].Action == DoorAction.Open ? TapeEventType.DoorOpened : TapeEventType.DoorClosed;
                events.Add(new TapeEvent(toggles[i].ExecuteTime, toggles[i].PawnId, type, toggles[i].Position));
            }
        }

        private void ResolveShot(
            ShotIntent shot,
            Dictionary<int, GhostTrack> tracks,
            List<int> order,
            List<TapeEvent> events,
            List<ResolvedHit> hits,
            ArenaBoard board)
        {
            GhostTrack shooter = tracks[shot.ShooterId];
            events.Add(new TapeEvent(shot.CompleteSeconds, shot.ShooterId, TapeEventType.ShootFire, shot.Aim,
                windowStartSeconds: shot.WindowStartSeconds));

            if (shot.Mode == ShootMode.HoldAngle)
            {
                ResolveHoldAngle(shot, shooter, tracks, order, hits, board);
                return;
            }

            ResolveSnapShot(shot, shooter, tracks, order, hits, board);
        }

        private static void ResolveSnapShot(
            ShotIntent shot,
            GhostTrack shooter,
            Dictionary<int, GhostTrack> tracks,
            List<int> order,
            List<ResolvedHit> hits,
            ArenaBoard board)
        {
            PlanarPosition origin = shooter.PositionAt(shot.CompleteSeconds);
            if (!ContinuousLineOfSight.HasLineOfSight(board, origin, shot.Aim))
            {
                return;
            }

            for (int i = 0; i < order.Count; i++)
            {
                int victimId = order[i];
                if (victimId == shot.ShooterId)
                {
                    continue;
                }

                GhostTrack victim = tracks[victimId];
                PlanarPosition victimPosition = victim.PositionAt(shot.CompleteSeconds);
                if (victimPosition.DistanceTo(shot.Aim) > HitRadius)
                {
                    continue;
                }

                // Aim-point LoS alone is not enough: HitRadius can reach a victim just past a closed
                // door when the aim sits short of the door on the shooter's side (playtest 2026-08-06).
                // Require clear LoS to the victim too — Hold Angle already did this at contact.
                if (!ContinuousLineOfSight.HasLineOfSight(board, origin, victimPosition))
                {
                    continue;
                }

                // Snap cannot catch a sprinting target (GDD §3A/§5).
                if (victim.StanceAt(shot.CompleteSeconds) == StanceType.Sprint)
                {
                    continue;
                }

                hits.Add(new ResolvedHit(shot.CompleteSeconds, shot.ShooterId, victimId, victimPosition, lethal: false));
            }
        }

        private void ResolveHoldAngle(
            ShotIntent shot,
            GhostTrack shooter,
            Dictionary<int, GhostTrack> tracks,
            List<int> order,
            List<ResolvedHit> hits,
            ArenaBoard board)
        {
            PlanarPosition origin = shooter.PositionAt(shot.WindowStartSeconds);

            for (int i = 0; i < order.Count; i++)
            {
                int victimId = order[i];
                if (victimId == shot.ShooterId)
                {
                    continue;
                }

                GhostTrack victim = tracks[victimId];
                if (!TryFindHoldContact(shot, origin, victim, board, out float contactSeconds, out PlanarPosition contactPosition))
                {
                    continue;
                }

                // Hold hits Sprint; lethal on contact (GDD §3A/§5).
                hits.Add(new ResolvedHit(contactSeconds, shot.ShooterId, victimId, contactPosition, lethal: true));
            }
        }

        /// <summary>
        /// Decision 5's analytic sweep. The victim's position is linear in time over each leg of its
        /// path, so both "is the projection onto the aim lane within [0,1]" and "is the perpendicular
        /// offset from the aim line within LaneHalfWidth" are affine functions of the leg parameter u —
        /// no sampling, no missed sweeps between probes.
        /// </summary>
        private static bool TryFindHoldContact(
            ShotIntent shot,
            PlanarPosition origin,
            GhostTrack victim,
            ArenaBoard board,
            out float contactSeconds,
            out PlanarPosition contactPosition)
        {
            contactSeconds = 0f;
            contactPosition = default;

            ScheduledPath path = victim.Path;
            IReadOnlyList<PlanarPosition> nodes = path.Nodes;
            IReadOnlyList<float> arrivals = path.ArrivalSeconds;

            for (int i = 1; i < nodes.Count; i++)
            {
                float legStart = arrivals[i - 1];
                float legEnd = arrivals[i];

                float windowLo = Math.Max(legStart, shot.WindowStartSeconds);
                float windowHi = Math.Min(legEnd, shot.CompleteSeconds);
                if (windowHi < windowLo)
                {
                    continue;
                }

                if (!TryEarliestContactOnLeg(
                        nodes[i - 1], legStart, nodes[i], legEnd,
                        windowLo, windowHi, origin, shot.Aim, LaneHalfWidth,
                        out float u))
                {
                    continue;
                }

                float seconds = legStart + (u * (legEnd - legStart));
                PlanarPosition position = PlanarPosition.Lerp(nodes[i - 1], nodes[i], u);

                // Known simplification: if LoS is blocked exactly at the earliest lane-covered instant
                // (e.g. a door mid-leg), this does not search later in the same leg for a moment where
                // it reopens — it moves on to the next leg instead. Acceptable for the 14-day scope
                // (only doors toggle mid-match; walls are static), flagged for Phase 6 tuning if it
                // turns out to matter in play.
                if (ContinuousLineOfSight.HasLineOfSight(board, origin, position))
                {
                    contactSeconds = seconds;
                    contactPosition = position;
                    return true;
                }
            }

            // A victim holds its final position after its last scheduled node — forever, if it has
            // none at all (e.g. an un-programmed defender: nodes.Count == 1, no legs above ever run).
            // The affine sweep above only covers explicit legs between nodes, so the trailing
            // stationary stretch needs its own check or Hold Angle can never catch a target who
            // simply stands still.
            float tailStart = arrivals[arrivals.Count - 1];
            float tailWindowLo = Math.Max(tailStart, shot.WindowStartSeconds);
            if (shot.CompleteSeconds >= tailWindowLo)
            {
                PlanarPosition tailPosition = nodes[nodes.Count - 1];
                if (ContinuousLineOfSight.IsOnLane(origin, shot.Aim, tailPosition, LaneHalfWidth)
                    && ContinuousLineOfSight.HasLineOfSight(board, origin, tailPosition))
                {
                    contactSeconds = tailWindowLo;
                    contactPosition = tailPosition;
                    return true;
                }
            }

            return false;
        }

        private static bool TryEarliestContactOnLeg(
            PlanarPosition legA, float legStart,
            PlanarPosition legB, float legEnd,
            float windowLo, float windowHi,
            PlanarPosition origin, PlanarPosition aim, float halfWidth,
            out float u)
        {
            u = 0f;
            float dt = legEnd - legStart;
            if (dt <= 1e-6f)
            {
                return false;
            }

            float uLo = Math.Max(0f, (windowLo - legStart) / dt);
            float uHi = Math.Min(1f, (windowHi - legStart) / dt);
            if (uHi < uLo)
            {
                return false;
            }

            var lane = new Segment(origin, aim);

            // t(u): projection param onto the aim lane — affine in u since position is affine in u.
            float t0 = lane.ProjectParam(legA);
            float t1 = lane.ProjectParam(legB);
            if (!TryAffineRangeWithin(t0, t1, 0f, 1f, out float tuLo, out float tuHi))
            {
                return false;
            }

            // signedPerp(u): perpendicular offset from the aim line — also affine in u.
            float p0 = SignedPerpDistance(origin, aim, legA);
            float p1 = SignedPerpDistance(origin, aim, legB);
            if (!TryAffineRangeWithin(p0, p1, -halfWidth, halfWidth, out float puLo, out float puHi))
            {
                return false;
            }

            float lo = Math.Max(uLo, Math.Max(tuLo, puLo));
            float hi = Math.Min(uHi, Math.Min(tuHi, puHi));
            if (hi < lo)
            {
                return false;
            }

            u = lo;
            return true;
        }

        /// <summary>
        /// f(u) = f0 + u*(f1-f0) is affine in u; returns the u-range (already intersected with [0,1])
        /// where f(u) stays within [min,max]. Same shape serves both the lane-span check and the
        /// half-width distance check below, since both are affine transforms of a position that is
        /// itself affine in u.
        /// </summary>
        private static bool TryAffineRangeWithin(float f0, float f1, float min, float max, out float uLo, out float uHi)
        {
            uLo = 0f;
            uHi = 1f;
            float slope = f1 - f0;
            if (Math.Abs(slope) <= 1e-9f)
            {
                return f0 >= min && f0 <= max;
            }

            float uAtMin = (min - f0) / slope;
            float uAtMax = (max - f0) / slope;
            uLo = Math.Max(0f, Math.Min(uAtMin, uAtMax));
            uHi = Math.Min(1f, Math.Max(uAtMin, uAtMax));
            return uHi >= uLo;
        }

        private static float SignedPerpDistance(PlanarPosition origin, PlanarPosition aim, PlanarPosition point)
        {
            float dx = aim.X - origin.X;
            float dy = aim.Y - origin.Y;
            float length = (float)Math.Sqrt((dx * dx) + (dy * dy));
            if (length <= 1e-6f)
            {
                return point.DistanceTo(origin);
            }

            float cross = (dx * (point.Y - origin.Y)) - (dy * (point.X - origin.X));
            return cross / length;
        }

        private static void ApplyHit(ResolvedHit hit, Dictionary<int, GhostTrack> tracks, List<TapeEvent> events)
        {
            GhostTrack victim = tracks[hit.VictimId];
            if (hit.Lethal)
            {
                victim.Wounds = WoundsUntilDead;
            }
            else
            {
                victim.Wounds++;
            }

            TapeEventType type = victim.Wounds >= WoundsUntilDead ? TapeEventType.Killed : TapeEventType.Wounded;
            events.Add(new TapeEvent(hit.Seconds, hit.VictimId, type, hit.Position, hit.ShooterId));
        }

        private static int CompareEvents(TapeEvent a, TapeEvent b)
        {
            int bySecond = a.Seconds.CompareTo(b.Seconds);
            if (bySecond != 0)
            {
                return bySecond;
            }

            int byType = ((int)a.Type).CompareTo((int)b.Type);
            return byType != 0 ? byType : a.PawnId.CompareTo(b.PawnId);
        }

        private readonly struct ShotIntent
        {
            public int ShooterId { get; }

            public float CompleteSeconds { get; }

            public float WindowStartSeconds { get; }

            public PlanarPosition Aim { get; }

            public ShootMode Mode { get; }

            public ShotIntent(int shooterId, float completeSeconds, float windowStartSeconds, PlanarPosition aim, ShootMode mode)
            {
                ShooterId = shooterId;
                CompleteSeconds = completeSeconds;
                WindowStartSeconds = windowStartSeconds;
                Aim = aim;
                Mode = mode;
            }
        }

        private readonly struct DoorToggleIntent
        {
            public int PawnId { get; }

            public float ExecuteTime { get; }

            public Door Door { get; }

            public PlanarPosition Position { get; }

            public DoorAction Action { get; }

            public DoorToggleIntent(int pawnId, float executeTime, Door door, PlanarPosition position, DoorAction action)
            {
                PawnId = pawnId;
                ExecuteTime = executeTime;
                Door = door;
                Position = position;
                Action = action;
            }
        }

        private readonly struct ResolvedHit
        {
            public float Seconds { get; }

            public int ShooterId { get; }

            public int VictimId { get; }

            public PlanarPosition Position { get; }

            public bool Lethal { get; }

            public ResolvedHit(float seconds, int shooterId, int victimId, PlanarPosition position, bool lethal)
            {
                Seconds = seconds;
                ShooterId = shooterId;
                VictimId = victimId;
                Position = position;
                Lethal = lethal;
            }
        }

        private sealed class GhostTrack
        {
            public GhostTrack(ScheduledPath path, IReadOnlyList<ActionNode> nodes, int startingWounds)
            {
                Path = path;
                Nodes = nodes;
                Wounds = startingWounds < 0 ? 0 : startingWounds;
            }

            public ScheduledPath Path { get; }

            public IReadOnlyList<ActionNode> Nodes { get; }

            public int Wounds { get; set; }

            public PlanarPosition PositionAt(float seconds)
            {
                return Path.Evaluate(seconds);
            }

            public StanceType StanceAt(float seconds)
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i].ExecuteTime >= seconds)
                    {
                        return Nodes[i].Stance;
                    }
                }

                return Nodes.Count > 0 ? Nodes[Nodes.Count - 1].Stance : StanceType.Walk;
            }
        }
    }
}
