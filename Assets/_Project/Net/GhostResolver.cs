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
    /// stand-in for the Fusion Host's authoritative ghost sim. Day 11 changes who calls this and how
    /// the tape travels, not what it computes.
    ///
    /// Resolve is a pure function of (board, inputs) — no UnityEngine.Time, no Random, no physics —
    /// so the same inputs always produce the same outcomes on every machine.
    ///
    /// Slice 1 rules, deliberately narrow:
    /// - <see cref="ActionNode.ExecuteTime"/> is a *completion* second, matching what PawnProgram
    ///   books: a Move arrives at it, a Snap Shot's window ends at it.
    /// - A Snap Shot resolves at that instant, and hits only a pawn standing on the aimed tile with
    ///   clear line of sight. Covering a line over time is Hold Angle's job (Day 6).
    /// - Simultaneous shots are grouped so a mutual exchange wounds both sides (paper D5 §IV).
    /// - A wound has no mechanical consequence yet: no surcharge, no re-timing, no bleed (Day 8).
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

            // Dictionary iteration order is not guaranteed, so every later pass walks sorted ids.
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

        /// <summary>
        /// Replays one pawn's nodes into a timed path. A Shoot contributes a waypoint at the pawn's
        /// current tile so its window is spent standing still, instead of the pawn drifting early
        /// into the following Move.
        /// </summary>
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
                    if (node.Verb == ActionVerb.Shoot)
                    {
                        shots.Add(new ShotIntent(order[i], node.ExecuteTime, node.GridPosition));
                    }
                }
            }

            shots.Sort((a, b) => a.Seconds != b.Seconds
                ? a.Seconds.CompareTo(b.Seconds)
                : a.ShooterId.CompareTo(b.ShooterId));

            var hits = new List<ResolvedHit>();
            int index = 0;
            while (index < shots.Count)
            {
                int groupEnd = index + 1;
                while (groupEnd < shots.Count && shots[groupEnd].Seconds - shots[index].Seconds <= _simultaneityEpsilon)
                {
                    groupEnd++;
                }

                // Every shot in the group is judged before any wound lands, so trading shots on the
                // same second is symmetric rather than first-id-wins.
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
            events.Add(new TapeEvent(shot.Seconds, shot.ShooterId, TapeEventType.ShootFire, shot.Target));

            GridCoordinate origin = shooter.TileAt(shot.Seconds);
            if (!GridLineOfSight.HasLineOfSight(_board, origin, shot.Target))
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
                if (victim.TileAt(shot.Seconds) != shot.Target)
                {
                    continue;
                }

                // A Snap Shot cannot catch a sprinting target (GDD §3A/§5). Unreachable while Slice 1
                // hardcodes Walk, but the rule belongs with the shot, not with the caller.
                if (victim.StanceAt(shot.Seconds) == StanceType.Sprint)
                {
                    continue;
                }

                hits.Add(new ResolvedHit(shot.Seconds, shot.ShooterId, victimId, shot.Target));
            }
        }

        private static void ApplyHit(ResolvedHit hit, Dictionary<int, GhostTrack> tracks, List<TapeEvent> events)
        {
            GhostTrack victim = tracks[hit.VictimId];
            victim.Wounds++;

            TapeEventType type = victim.Wounds >= WoundsUntilDead ? TapeEventType.Killed : TapeEventType.Wounded;
            events.Add(new TapeEvent(hit.Seconds, hit.VictimId, type, hit.Tile, hit.ShooterId));
        }

        /// <summary>Causal order at a shared second: arrivals, then fire, then its consequences.</summary>
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

            public float Seconds { get; }

            public GridCoordinate Target { get; }

            public ShotIntent(int shooterId, float seconds, GridCoordinate target)
            {
                ShooterId = shooterId;
                Seconds = seconds;
                Target = target;
            }
        }

        private readonly struct ResolvedHit
        {
            public float Seconds { get; }

            public int ShooterId { get; }

            public int VictimId { get; }

            public GridCoordinate Tile { get; }

            public ResolvedHit(float seconds, int shooterId, int victimId, GridCoordinate tile)
            {
                Seconds = seconds;
                ShooterId = shooterId;
                VictimId = victimId;
                Tile = tile;
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
