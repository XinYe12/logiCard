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

            // C67 — Storm card cast. Continuous presenter mirrors DoorOpened/Closed
            // (RoundPlayback.SyncWeatherToSeconds): board weather is Storm from this event's
            // Seconds onward for the rest of the round.
            TapeEventType.StormCast,

            // C63 — Bandage resolve lands here. One-shot banner only (RoundPlayback.Report) — no
            // board splat to hide, since a Healed event can only ever clear a wound carried in from
            // a prior round (GhostResolver.CompileTrack resolves it from GhostInput.StartingWounds
            // before this round's own ResolveShots pass runs), and BuildHitVfx only ever splats this
            // round's own Wounded/Killed events. See TapeEvent.cs's doc comment on this value.
            TapeEventType.Healed,

            // C36/C71 — Bomber wall-only v1 presenter (2026-08-20, landed after the Sim-layer-only
            // slice). BombAttached: one-shot banner only (RoundPlayback.Report), same shape as Healed.
            // GeometryBreached: continuous presenter mirroring DoorOpened/Closed exactly
            // (RoundPlayback.SyncBreachToSeconds) — breach point is Breached from this event's Seconds
            // onward, rewind-safe via the same arm-snapshot-plus-forward-scan shape SyncDoorsToSeconds
            // uses. BoardView breach-point visuals and map authoring of real breach points remain
            // separate follow-up work (see docs/contracts/CURRENT.md's C36 section) — the board model
            // is fully live, nothing renders it yet.
            TapeEventType.BombAttached,
            TapeEventType.GeometryBreached,
        };

        private static readonly HashSet<TapeEventType> ReservedNoPresenterYet = new HashSet<TapeEventType>
        {
            TapeEventType.Invalid,
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
