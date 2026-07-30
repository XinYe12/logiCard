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

        public IReadOnlyDictionary<int, ScheduledPath> Tracks { get; }

        /// <summary>Ascending by Time Resource second, ties broken by pawn id for determinism.</summary>
        public IReadOnlyList<TapeEvent> Events { get; }

        /// <summary>Wound counts at end of resolve, including any StartingWounds carried in (C33).</summary>
        public IReadOnlyDictionary<int, int> EndWounds { get; }

        public float EndSeconds { get; }

        public ReplayTape(
            IReadOnlyDictionary<int, ScheduledPath> tracks,
            IReadOnlyList<TapeEvent> events,
            IReadOnlyDictionary<int, int> endWounds = null)
        {
            Tracks = tracks ?? new Dictionary<int, ScheduledPath>();
            Events = events ?? new List<TapeEvent>();
            EndWounds = endWounds ?? NoWounds;

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
    }
}
