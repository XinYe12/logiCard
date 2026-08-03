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

        public GridCoordinate Start { get; }

        public TimelinePayload Payload { get; }

        /// <summary>Wounds carried in from prior rounds (C33). Zero on the first round.</summary>
        public int StartingWounds { get; }

        public GhostInput(int pawnId, GridCoordinate start, TimelinePayload payload, int startingWounds = 0)
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
    /// Combat (Days 4–6):
    /// - <see cref="ActionNode.ExecuteTime"/> is a *completion* second.
    /// - Snap Shot: aimed tile only at completion (C32); wounds; misses Sprint.
    /// - Hold Angle: covers the aim lane across its window; lethal; hits Sprint.
    /// - Simultaneous shots in a group are judged before wounds apply (mutual exchange / mutual lethal).
    /// </summary>
    public sealed class GhostResolver
    {
        public const int WoundsUntilDead = 2;

        private const float DefaultSimultaneityEpsilon = 0.01f;
        private static readonly IReadOnlyList<ActionNode> NoNodes = new List<ActionNode>();

        private readonly GridBoard _board;
        private readonly float _simultaneityEpsilon;

        public GhostResolver(GridBoard board, float simultaneityEpsilon = DefaultSimultaneityEpsilon)
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

            var waypoints = new List<GridCoordinate> { input.Start };
            var arrivals = new List<float> { 0f };
            GridCoordinate current = input.Start;

            foreach (ActionNode node in ordered)
            {
                if (node.Verb == ActionVerb.Move)
                {
                    current = node.GridPosition;
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
            for (int i = 0; i < order.Count; i++)
            {
                GhostTrack track = tracks[order[i]];
                foreach (ActionNode node in track.Nodes)
                {
                    if (node.Verb != ActionVerb.Shoot)
                    {
                        continue;
                    }

                    ShootMode mode = node.ShootMode == ShootMode.None ? ShootMode.SnapShot : node.ShootMode;
                    float cost = ShootCost.SecondsFor(mode);
                    float windowStart = node.ExecuteTime - cost;
                    if (windowStart < 0f)
                    {
                        windowStart = 0f;
                    }

                    shots.Add(new ShotIntent(order[i], node.ExecuteTime, windowStart, node.GridPosition, mode));
                }
            }

            // Sort by resolve instant: Snap at completion; Hold also stamps consequences at first
            // contact, but the fire event is still listed at completion for scrubber readability.
            shots.Sort((a, b) => a.CompleteSeconds != b.CompleteSeconds
                ? a.CompleteSeconds.CompareTo(b.CompleteSeconds)
                : a.ShooterId.CompareTo(b.ShooterId));

            var hits = new List<ResolvedHit>();
            int index = 0;
            while (index < shots.Count)
            {
                int groupEnd = index + 1;
                while (groupEnd < shots.Count
                       && shots[groupEnd].CompleteSeconds - shots[index].CompleteSeconds <= _simultaneityEpsilon)
                {
                    groupEnd++;
                }

                hits.Clear();
                for (int i = index; i < groupEnd; i++)
                {
                    ResolveShot(shots[i], tracks, order, events, hits);
                }

                for (int i = 0; i < hits.Count; i++)
                {
                    ApplyHit(hits[i], tracks, events);
                }

                index = groupEnd;
            }
        }

        private void ResolveShot(
            ShotIntent shot,
            Dictionary<int, GhostTrack> tracks,
            List<int> order,
            List<TapeEvent> events,
            List<ResolvedHit> hits)
        {
            GhostTrack shooter = tracks[shot.ShooterId];
            events.Add(new TapeEvent(shot.CompleteSeconds, shot.ShooterId, TapeEventType.ShootFire, shot.Aim));

            if (shot.Mode == ShootMode.HoldAngle)
            {
                ResolveHoldAngle(shot, shooter, tracks, order, hits);
                return;
            }

            ResolveSnapShot(shot, shooter, tracks, order, hits);
        }

        private void ResolveSnapShot(
            ShotIntent shot,
            GhostTrack shooter,
            Dictionary<int, GhostTrack> tracks,
            List<int> order,
            List<ResolvedHit> hits)
        {
            GridCoordinate origin = shooter.TileAt(shot.CompleteSeconds);
            if (!GridLineOfSight.HasLineOfSight(_board, origin, shot.Aim))
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
                if (victim.TileAt(shot.CompleteSeconds) != shot.Aim)
                {
                    continue;
                }

                // Snap cannot catch a sprinting target (GDD §3A/§5).
                if (victim.StanceAt(shot.CompleteSeconds) == StanceType.Sprint)
                {
                    continue;
                }

                hits.Add(new ResolvedHit(shot.CompleteSeconds, shot.ShooterId, victimId, shot.Aim, lethal: false));
            }
        }

        private void ResolveHoldAngle(
            ShotIntent shot,
            GhostTrack shooter,
            Dictionary<int, GhostTrack> tracks,
            List<int> order,
            List<ResolvedHit> hits)
        {
            GridCoordinate origin = shooter.TileAt(shot.WindowStartSeconds);

            for (int i = 0; i < order.Count; i++)
            {
                int victimId = order[i];
                if (victimId == shot.ShooterId)
                {
                    continue;
                }

                GhostTrack victim = tracks[victimId];
                if (!TryFindHoldContact(shot, origin, victim, out float contactSeconds, out GridCoordinate contactTile))
                {
                    continue;
                }

                // Hold hits Sprint; lethal on contact (GDD §3A/§5).
                hits.Add(new ResolvedHit(contactSeconds, shot.ShooterId, victimId, contactTile, lethal: true));
            }
        }

        private bool TryFindHoldContact(
            ShotIntent shot,
            GridCoordinate origin,
            GhostTrack victim,
            out float contactSeconds,
            out GridCoordinate contactTile)
        {
            contactSeconds = 0f;
            contactTile = default;

            var probes = new List<float> { shot.WindowStartSeconds, shot.CompleteSeconds };
            foreach (ActionNode node in victim.Nodes)
            {
                if (node.Verb != ActionVerb.Move)
                {
                    continue;
                }

                if (node.ExecuteTime > shot.WindowStartSeconds && node.ExecuteTime < shot.CompleteSeconds)
                {
                    probes.Add(node.ExecuteTime);
                }
            }

            probes.Sort();

            for (int i = 0; i < probes.Count; i++)
            {
                if (i > 0 && probes[i] - probes[i - 1] <= _simultaneityEpsilon)
                {
                    continue;
                }

                float t = probes[i];
                GridCoordinate tile = victim.TileAt(t);
                if (!GridLineOfSight.IsOnCoveredLane(origin, shot.Aim, tile))
                {
                    continue;
                }

                if (!GridLineOfSight.HasLineOfSight(_board, origin, tile))
                {
                    continue;
                }

                contactSeconds = t;
                contactTile = tile;
                return true;
            }

            return false;
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
            events.Add(new TapeEvent(hit.Seconds, hit.VictimId, type, hit.Tile, hit.ShooterId));
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

            public GridCoordinate Aim { get; }

            public ShootMode Mode { get; }

            public ShotIntent(int shooterId, float completeSeconds, float windowStartSeconds, GridCoordinate aim, ShootMode mode)
            {
                ShooterId = shooterId;
                CompleteSeconds = completeSeconds;
                WindowStartSeconds = windowStartSeconds;
                Aim = aim;
                Mode = mode;
            }
        }

        private readonly struct ResolvedHit
        {
            public float Seconds { get; }

            public int ShooterId { get; }

            public int VictimId { get; }

            public GridCoordinate Tile { get; }

            public bool Lethal { get; }

            public ResolvedHit(float seconds, int shooterId, int victimId, GridCoordinate tile, bool lethal)
            {
                Seconds = seconds;
                ShooterId = shooterId;
                VictimId = victimId;
                Tile = tile;
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

            public GridCoordinate TileAt(float seconds)
            {
                return Path.Evaluate(seconds).ToNearestCoordinate();
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
