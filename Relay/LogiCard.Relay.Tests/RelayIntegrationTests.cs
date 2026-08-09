using System.Collections.Generic;
using System.Threading.Tasks;
using LogiCard.Net;
using LogiCard.Sim;
using Xunit;

namespace LogiCard.Relay.Tests
{
    /// <summary>
    /// Phase 2 exit proof without Unity: two plain .NET clients ↔ real TCP relay ↔ identical
    /// ReplayTape that matches an in-process GhostResolver.Resolve of the same inputs.
    /// </summary>
    public sealed class RelayIntegrationTests
    {
        private const int Attacker = 1;
        private const int Defender = 2;

        [Fact]
        public async Task TwoClients_ReceiveIdenticalTape_MatchingLocalResolve()
        {
            ArenaBoard board = DemoArenaBoard.CreateEmpty();
            var localResolver = new GhostResolver(board);

            GhostInput attackerInput = new GhostInput(
                Attacker,
                new PlanarPosition(0f, 0f),
                new TimelinePayload(new List<ActionNode>
                {
                    new ActionNode(ActionVerb.Move, 4f, new PlanarPosition(0f, 2f), StanceType.Walk),
                    new ActionNode(ActionVerb.Shoot, 6f, new PlanarPosition(2f, 2f), StanceType.Walk,
                        modifier: null, ShootMode.SnapShot),
                }));

            GhostInput defenderInput = new GhostInput(
                Defender,
                new PlanarPosition(2f, 2f),
                new TimelinePayload(new List<ActionNode>
                {
                    new ActionNode(ActionVerb.Move, 3f, new PlanarPosition(2f, 1f), StanceType.Walk),
                }));

            ReplayTape expected = localResolver.Resolve(new[] { attackerInput, defenderInput });

            using var server = new RelayServer(port: 0, board);
            server.Start();
            int port = server.Port;

            Task<ReplayTape> clientA = Task.Run(() =>
            {
                using var client = new RelayRoundClient("127.0.0.1", port);
                return client.SubmitAndWait(new[] { attackerInput });
            });

            Task<ReplayTape> clientB = Task.Run(() =>
            {
                using var client = new RelayRoundClient("127.0.0.1", port);
                return client.SubmitAndWait(new[] { defenderInput });
            });

            ReplayTape[] tapes = await Task.WhenAll(clientA, clientB);
            await server.Completion;

            TapeAssert.Equal(tapes[0], tapes[1]);
            TapeAssert.Equal(expected, tapes[0]);
            TapeAssert.Equal(expected, tapes[1]);
        }

        [Fact]
        public async Task DemoBoard_DoorToggle_MatchesLocalResolve()
        {
            ArenaBoard board = DemoArenaBoard.CreateDemo();
            var localResolver = new GhostResolver(board);

            // Attacker walks to Door #1 interaction point and opens it; defender holds.
            var doorMid = new PlanarPosition(4f, 4f);
            GhostInput attackerInput = new GhostInput(
                Attacker,
                new PlanarPosition(4f, 0f),
                new TimelinePayload(new List<ActionNode>
                {
                    new ActionNode(ActionVerb.Move, 8f, new PlanarPosition(4f, 3.5f), StanceType.Walk),
                    new ActionNode(ActionVerb.Door, 9f, doorMid, StanceType.Walk,
                        modifier: null, ShootMode.None, DoorAction.Open),
                }));

            GhostInput defenderInput = new GhostInput(
                Defender,
                new PlanarPosition(4f, 6f),
                new TimelinePayload(new List<ActionNode>()));

            ReplayTape expected = localResolver.Resolve(new[] { attackerInput, defenderInput });

            using var server = new RelayServer(port: 0, board);
            server.Start();
            int port = server.Port;

            Task<ReplayTape> a = Task.Run(() =>
            {
                using var client = new RelayRoundClient("127.0.0.1", port);
                return client.SubmitAndWait(new[] { attackerInput });
            });
            Task<ReplayTape> b = Task.Run(() =>
            {
                using var client = new RelayRoundClient("127.0.0.1", port);
                return client.SubmitAndWait(new[] { defenderInput });
            });

            ReplayTape[] tapes = await Task.WhenAll(a, b);
            await server.Completion;

            TapeAssert.Equal(expected, tapes[0]);
            TapeAssert.Equal(tapes[0], tapes[1]);
        }
    }
}
