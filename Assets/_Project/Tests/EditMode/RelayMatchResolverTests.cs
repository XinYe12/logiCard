using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LogiCard.Net;
using LogiCard.Sim;
using NUnit.Framework;

namespace LogiCard.Tests.EditMode
{
    /// <summary>
    /// Unity-side coverage for <see cref="RelayMatchResolver"/>: a lightweight loopback stub
    /// (not the full Relay/ process) proves the coroutine yields while waiting and resolves once
    /// the tape frame arrives. Two full Editor processes are not required.
    /// </summary>
    [TestFixture]
    public sealed class RelayMatchResolverTests
    {
        [Test]
        public void ResolveAsync_YieldsWhileWaiting_ThenDeliversTape()
        {
            var expectedNodes = new List<ActionNode>
            {
                new ActionNode(ActionVerb.Move, 2f, new PlanarPosition(1f, 1f), StanceType.Walk),
            };
            var expectedTape = new ReplayTape(
                new Dictionary<int, ScheduledPath>
                {
                    {
                        1,
                        ScheduledPath.FromTimedWaypoints(
                            new[] { new PlanarPosition(0f, 0f), new PlanarPosition(1f, 1f) },
                            new[] { 0f, 2f })
                    },
                },
                new List<TapeEvent>
                {
                    new TapeEvent(2f, 1, TapeEventType.MoveArrive, new PlanarPosition(1f, 1f)),
                },
                new Dictionary<int, int> { { 1, 0 } });

            using var stub = new StubRelayServer(expectedTape);
            stub.Start();

            var resolver = new RelayMatchResolver("127.0.0.1", stub.Port);
            var input = new GhostInput(1, new PlanarPosition(0f, 0f), new TimelinePayload(expectedNodes));

            ReplayTape resolved = null;
            IEnumerator routine = resolver.ResolveAsync(new[] { input }, tape => resolved = tape);

            int steps = 0;
            bool yielded = false;
            while (routine.MoveNext())
            {
                yielded = true;
                steps++;
                // Background socket I/O; give the thread pool a slice without blocking the test forever.
                Thread.Sleep(5);
                Assert.Less(steps, 4000, "ResolveAsync never completed against the stub relay.");
            }

            Assert.That(yielded, Is.True, "Expected at least one yield while network I/O was in flight.");
            Assert.That(resolved, Is.Not.Null);
            Assert.That(resolved.EndSeconds, Is.EqualTo(expectedTape.EndSeconds).Within(0.0001f));
            Assert.That(resolved.Events.Count, Is.EqualTo(1));
            Assert.That(resolved.Events[0].Type, Is.EqualTo(TapeEventType.MoveArrive));
            Assert.That(resolved.Tracks[1].Evaluate(2f), Is.EqualTo(new PlanarPosition(1f, 1f)));
            Assert.That(stub.ReceivedInputCount, Is.EqualTo(1));
        }

        [Test]
        public void ResolveAsync_PropagatesRelayError()
        {
            using var stub = new StubRelayServer(errorMessage: "boom");
            stub.Start();

            var resolver = new RelayMatchResolver("127.0.0.1", stub.Port);
            var input = new GhostInput(1, new PlanarPosition(0f, 0f), new TimelinePayload(new List<ActionNode>()));

            IEnumerator routine = resolver.ResolveAsync(new[] { input }, _ => { });
            InvalidOperationException thrown = null;
            int steps = 0;
            try
            {
                while (routine.MoveNext())
                {
                    Thread.Sleep(5);
                    Assert.Less(steps++, 4000);
                }
            }
            catch (InvalidOperationException ex)
            {
                thrown = ex;
            }

            Assert.That(thrown, Is.Not.Null);
            Assert.That(thrown.Message, Does.Contain("boom").Or.Contain("Relay"));
        }

        /// <summary>Minimal one-client TCP responder speaking <see cref="RelayProtocol"/>.</summary>
        private sealed class StubRelayServer : IDisposable
        {
            private readonly ReplayTape _tape;
            private readonly string _errorMessage;
            private readonly TcpListener _listener;
            private CancellationTokenSource _cts;
            private Task _loop;

            public StubRelayServer(ReplayTape tape)
            {
                _tape = tape;
                _listener = new TcpListener(IPAddress.Loopback, 0);
            }

            public StubRelayServer(string errorMessage)
            {
                _errorMessage = errorMessage;
                _listener = new TcpListener(IPAddress.Loopback, 0);
            }

            public int Port { get; private set; }

            public int ReceivedInputCount { get; private set; }

            public void Start()
            {
                _cts = new CancellationTokenSource();
                _listener.Start();
                Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
                _loop = Task.Run(() => ServeAsync(_cts.Token));
            }

            private async Task ServeAsync(CancellationToken cancellationToken)
            {
                using TcpClient client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                NetworkStream stream = client.GetStream();
                byte[] request = await RelayProtocol.ReadFrameAsync(stream, cancellationToken).ConfigureAwait(false);
                RelayEnvelope submit = RelayProtocol.DeserializeEnvelope(request);
                ReceivedInputCount = submit.Inputs != null ? submit.Inputs.Length : 0;

                RelayEnvelope response = _errorMessage != null
                    ? RelayProtocol.MakeError(_errorMessage)
                    : RelayProtocol.MakeTape(_tape);
                await RelayProtocol.WriteFrameAsync(stream, RelayProtocol.SerializeEnvelope(response), cancellationToken)
                    .ConfigureAwait(false);
            }

            public void Dispose()
            {
                _cts?.Cancel();
                try
                {
                    _listener.Stop();
                }
                catch (SocketException)
                {
                }

                _cts?.Dispose();
            }
        }
    }
}
