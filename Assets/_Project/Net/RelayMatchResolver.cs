using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace LogiCard.Net
{
    /// <summary>
    /// Networked <see cref="IMatchResolver"/> (C52 Phase 2 first slice): sends this side's
    /// <see cref="GhostInput"/>s to the standalone resolve-relay over TCP and yields until the
    /// authoritative <see cref="ReplayTape"/> returns. Socket I/O runs on a background thread so
    /// Unity's main thread stays free; the coroutine polls a completion flag with
    /// <c>yield return null</c>.
    ///
    /// Not wired into <c>GameBootstrap</c> here — Integrator owns that seam at merge time.
    /// </summary>
    public sealed class RelayMatchResolver : IMatchResolver
    {
        private readonly string _host;
        private readonly int _port;

        public RelayMatchResolver(string host = "127.0.0.1", int port = RelayProtocol.DefaultPort)
        {
            _host = string.IsNullOrWhiteSpace(host) ? "127.0.0.1" : host;
            _port = port;
        }

        public string Host => _host;

        public int Port => _port;

        public IEnumerator ResolveAsync(IReadOnlyList<GhostInput> inputs, Action<ReplayTape> onResolved)
        {
            if (onResolved == null)
            {
                throw new ArgumentNullException(nameof(onResolved));
            }

            var captured = new List<GhostInput>();
            if (inputs != null)
            {
                for (int i = 0; i < inputs.Count; i++)
                {
                    captured.Add(inputs[i]);
                }
            }

            ReplayTape tape = null;
            Exception error = null;
            int done = 0;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    using (var client = new RelayRoundClient(_host, _port))
                    {
                        tape = client.SubmitAndWait(captured);
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                finally
                {
                    Interlocked.Exchange(ref done, 1);
                }
            });

            while (Volatile.Read(ref done) == 0)
            {
                yield return null;
            }

            if (error != null)
            {
                throw new InvalidOperationException(
                    $"RelayMatchResolver failed talking to {_host}:{_port}: {error.Message}", error);
            }

            onResolved(tape);
        }
    }
}
