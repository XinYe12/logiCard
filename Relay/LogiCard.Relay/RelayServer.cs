using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LogiCard.Net;
using LogiCard.Sim;

namespace LogiCard.Relay
{
    /// <summary>
    /// Minimal first-slice relay: accept exactly two TCP clients, wait for each side's Submit,
    /// run <see cref="GhostResolver.Resolve"/> once, send the identical Tape to both.
    /// No matchmaking queue, no persistence, no reconnect — see NETWORKING_DESIGN.md OPEN table.
    /// </summary>
    public sealed class RelayServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly GhostResolver _resolver;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Task _runTask;
        private int _started;

        public RelayServer(int port, ArenaBoard board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            _listener = new TcpListener(IPAddress.Loopback, port);
            _resolver = new GhostResolver(board);
        }

        /// <summary>Bound port after <see cref="Start"/> (useful when constructed with port 0).</summary>
        public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

        public void Start()
        {
            if (Interlocked.Exchange(ref _started, 1) != 0)
            {
                return;
            }

            _listener.Start();
            _runTask = Task.Run(() => RunAsync(_cts.Token));
        }

        public Task Completion => _runTask ?? Task.CompletedTask;

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"[relay] listening on 127.0.0.1:{Port}");

            using TcpClient clientA = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
            Console.WriteLine("[relay] client A connected");
            using TcpClient clientB = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
            Console.WriteLine("[relay] client B connected — match paired");

            Task<List<GhostInput>> submitA = ReadSubmitAsync(clientA, cancellationToken);
            Task<List<GhostInput>> submitB = ReadSubmitAsync(clientB, cancellationToken);
            List<GhostInput>[] both = await Task.WhenAll(submitA, submitB).ConfigureAwait(false);

            var combined = new List<GhostInput>();
            combined.AddRange(both[0]);
            combined.AddRange(both[1]);
            Console.WriteLine($"[relay] resolving {combined.Count} GhostInput(s)");

            ReplayTape tape = _resolver.Resolve(combined);
            byte[] frame = RelayProtocol.SerializeEnvelope(RelayProtocol.MakeTape(tape));

            await Task.WhenAll(
                RelayProtocol.WriteFrameAsync(clientA.GetStream(), frame, cancellationToken),
                RelayProtocol.WriteFrameAsync(clientB.GetStream(), frame, cancellationToken))
                .ConfigureAwait(false);

            Console.WriteLine($"[relay] tape sent to both (EndSeconds={tape.EndSeconds:0.###}, events={tape.Events.Count})");
        }

        private static async Task<List<GhostInput>> ReadSubmitAsync(TcpClient client, CancellationToken cancellationToken)
        {
            byte[] payload = await RelayProtocol.ReadFrameAsync(client.GetStream(), cancellationToken)
                .ConfigureAwait(false);
            RelayEnvelope envelope = RelayProtocol.DeserializeEnvelope(payload);
            if (envelope.Type != RelayProtocol.MessageSubmit)
            {
                throw new InvalidDataException("Expected Submit from client, got: " + envelope.Type);
            }

            var inputs = new List<GhostInput>();
            if (envelope.Inputs != null)
            {
                for (int i = 0; i < envelope.Inputs.Length; i++)
                {
                    inputs.Add(envelope.Inputs[i].ToDomain());
                }
            }

            return inputs;
        }

        public void Dispose()
        {
            _cts.Cancel();
            try
            {
                _listener.Stop();
            }
            catch (SocketException)
            {
                // already stopped
            }

            _cts.Dispose();
        }
    }
}
