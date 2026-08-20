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
        };

        private static readonly HashSet<TapeEventType> ReservedNoPresenterYet = new HashSet<TapeEventType>
        {
            TapeEventType.Invalid,

            // C36/C71 — Bomber wall-only v1 (2026-08-20). Sim-layer-only slice: GhostResolver emits
            // both events correctly (see GhostResolverBombTests), but RoundPlayback presenter wiring,
            // BoardView breach-point visuals, and map authoring of real breach points are explicit
            // follow-up work, not built yet. Move to PresentedAtScrubber once RoundPlayback wires them
            // (GeometryBreached should mirror DoorOpened/Closed exactly; BombAttached likely stays a
            // one-shot banner or board-anchored marker, no continuous geometry change of its own).
            TapeEventType.BombAttached,
            TapeEventType.GeometryBreached,
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
