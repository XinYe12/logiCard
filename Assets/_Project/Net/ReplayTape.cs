using System.Collections.Generic;
using LogiCard.Sim;

namespace LogiCard.Net
{
    /// <summary>
    /// The immutable result of a resolve (C23): continuous ghost positions plus the ordered outcomes
    /// that happened along them. Playback only reads this — it never re-simulates — which is what
    /// makes Day 11's transport swap unable to change what players see.
    /// </summary>
    public sealed class ReplayTape
    {
        private static readonly IReadOnlyDictionary<int, int> NoWounds = new Dictionary<int, int>();
        private static readonly IReadOnlyDictionary<int, int> NoBandageCharge = new Dictionary<int, int>();
        private static readonly IReadOnlyDictionary<int, int> NoStormCastCount = new Dictionary<int, int>();

        public IReadOnlyDictionary<int, ScheduledPath> Tracks { get; }

        /// <summary>Ascending by Time Resource second, ties broken by pawn id for determinism.</summary>
        public IReadOnlyList<TapeEvent> Events { get; }

        /// <summary>Wound counts at end of resolve, including any StartingWounds carried in (C33).</summary>
        public IReadOnlyDictionary<int, int> EndWounds { get; }

        /// <summary>Bandage charges spent (0 or 1) at end of resolve, including any StartingBandageCharge
        /// carried in (C63). Mirrors <see cref="EndWounds"/>'s per-match carry-forward shape.</summary>
        public IReadOnlyDictionary<int, int> EndBandageCharge { get; }

        /// <summary>Storm casts spent (0 or 1) at end of resolve, including any StartingStormCastCount
        /// carried in (C69). Mirrors <see cref="EndBandageCharge"/>'s shape exactly.</summary>
        public IReadOnlyDictionary<int, int> EndStormCastCount { get; }

        public float EndSeconds { get; }

        public ReplayTape(
            IReadOnlyDictionary<int, ScheduledPath> tracks,
            IReadOnlyList<TapeEvent> events,
            IReadOnlyDictionary<int, int> endWounds = null,
            IReadOnlyDictionary<int, int> endBandageCharge = null,
            IReadOnlyDictionary<int, int> endStormCastCount = null)
        {
            Tracks = tracks ?? new Dictionary<int, ScheduledPath>();
            Events = events ?? new List<TapeEvent>();
            EndWounds = endWounds ?? NoWounds;
            EndBandageCharge = endBandageCharge ?? NoBandageCharge;
            EndStormCastCount = endStormCastCount ?? NoStormCastCount;

            float end = 0f;
            foreach (KeyValuePair<int, ScheduledPath> track in Tracks)
            {
                if (track.Value != null && track.Value.EndSeconds > end)
                {
                    end = track.Value.EndSeconds;
                }
            }

            for (int i = 0; i < Events.Count; i++)
            {
                if (Events[i].Seconds > end)
                {
                    end = Events[i].Seconds;
                }
            }

            EndSeconds = end;
        }

        public bool AnyoneDead(int woundsUntilDead = GhostResolver.WoundsUntilDead)
        {
            foreach (KeyValuePair<int, int> entry in EndWounds)
            {
                if (entry.Value >= woundsUntilDead)
                {
                    return true;
                }
            }

            return false;
        }

        public int WoundsFor(int pawnId)
        {
            return EndWounds.TryGetValue(pawnId, out int wounds) ? wounds : 0;
        }

        public int BandageChargeFor(int pawnId)
        {
            return EndBandageCharge.TryGetValue(pawnId, out int charge) ? charge : 0;
        }

        public int StormCastCountFor(int pawnId)
        {
            return EndStormCastCount.TryGetValue(pawnId, out int count) ? count : 0;
        }
    }
}
