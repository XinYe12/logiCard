using System;
using System.Collections.Generic;
using LogiCard.Net;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// PLAYBACK_CONTRACT §5 — when <see cref="TapeEventType"/> grows, someone must wire a presenter
    /// (or mark the value reserved) before merge.
    /// </summary>
    [TestFixture]
    public sealed class TapeEventPlaybackCoverageTests
    {
        private static readonly HashSet<TapeEventType> PresentedAtScrubber = new HashSet<TapeEventType>
        {
            TapeEventType.MoveArrive,
            TapeEventType.ShootFire,
            TapeEventType.Wounded,
            TapeEventType.Killed,
            TapeEventType.DoorOpened,
            TapeEventType.DoorClosed,
        };

        private static readonly HashSet<TapeEventType> ReservedNoPresenterYet = new HashSet<TapeEventType>
        {
            TapeEventType.Invalid,

            // C63 — Bandage resolve lands here; the Healed presenter (hide/restore the specific
            // wound splat it clears) is an explicit Integrator follow-up per
            // docs/contracts/CURRENT.md's Bandage contract, not part of this slot. Move to
            // PresentedAtScrubber once RoundPlayback wires it.
            TapeEventType.Healed,
        };

        [Test]
        public void EveryTapeEventTypeIsPresentedOrExplicitlyReserved()
        {
            foreach (TapeEventType type in Enum.GetValues(typeof(TapeEventType)))
            {
                bool ok = PresentedAtScrubber.Contains(type) || ReservedNoPresenterYet.Contains(type);
                Assert.That(
                    ok,
                    Is.True,
                    $"TapeEventType.{type} has no Playback presenter mapping — update RoundPlayback + docs/PLAYBACK_CONTRACT.md §3/§5.");
            }
        }
    }
}
