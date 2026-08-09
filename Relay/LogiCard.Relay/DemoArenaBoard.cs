using LogiCard.Sim;

namespace LogiCard.Relay
{
    /// <summary>
    /// Board layouts the relay can resolve against. First-slice scope has no board-sync handshake
    /// (still OPEN in NETWORKING_DESIGN.md) — the relay and clients must agree out-of-band.
    /// <see cref="CreateDemo"/> mirrors <c>GameBootstrap.BuildBoard</c> so a two-Unity smoke test
    /// against the shipped layout produces the same tape the local resolver would.
    /// </summary>
    public static class DemoArenaBoard
    {
        public static ArenaBoard CreateEmpty()
        {
            return new ArenaBoard(floors: new[] { Floor.Ground });
        }

        /// <summary>Same multi-room layout as GameBootstrap (Yard / Hall / Vault, two closed doors).</summary>
        public static ArenaBoard CreateDemo()
        {
            var model = new ArenaBoard(0f, 0f, 8f, 10f, new[] { Floor.Ground });

            model.RegisterWall(new Segment(new PlanarPosition(2f, 4f), new PlanarPosition(3.75f, 4f)));
            model.RegisterWall(new Segment(new PlanarPosition(4.25f, 4f), new PlanarPosition(6f, 4f)));
            model.RegisterDoor(new Door(
                new Segment(new PlanarPosition(3.75f, 4f), new PlanarPosition(4.25f, 4f)),
                DoorState.Closed,
                displayName: "Door #1"));

            model.RegisterWall(new Segment(new PlanarPosition(2f, 4f), new PlanarPosition(2f, 7f)));
            model.RegisterWall(new Segment(new PlanarPosition(6f, 4f), new PlanarPosition(6f, 7f)));

            model.RegisterWall(new Segment(new PlanarPosition(2f, 7f), new PlanarPosition(3.75f, 7f)));
            model.RegisterWall(new Segment(new PlanarPosition(4.25f, 7f), new PlanarPosition(6f, 7f)));
            model.RegisterDoor(new Door(
                new Segment(new PlanarPosition(3.75f, 7f), new PlanarPosition(4.25f, 7f)),
                DoorState.Closed,
                displayName: "Door #2"));

            return model;
        }
    }
}
