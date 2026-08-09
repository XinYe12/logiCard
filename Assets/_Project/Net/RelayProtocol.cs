using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LogiCard.Sim;

namespace LogiCard.Net
{
    /// <summary>
    /// Wire protocol for the Phase 2 resolve-relay (C52): length-prefixed JSON over TCP.
    /// Shared by the standalone relay process (via <c>Compile Include</c>) and
    /// <see cref="RelayMatchResolver"/> so client/server never drift.
    /// </summary>
    public static class RelayProtocol
    {
        public const int DefaultPort = 7777;
        public const string MessageSubmit = "Submit";
        public const string MessageTape = "Tape";
        public const string MessageError = "Error";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        public static byte[] SerializeEnvelope(RelayEnvelope envelope)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope, JsonOptions));
        }

        public static RelayEnvelope DeserializeEnvelope(byte[] payload)
        {
            RelayEnvelope envelope = JsonSerializer.Deserialize<RelayEnvelope>(payload, JsonOptions);
            if (envelope == null || string.IsNullOrEmpty(envelope.Type))
            {
                throw new InvalidDataException("Relay envelope missing type.");
            }

            return envelope;
        }

        public static async Task WriteFrameAsync(Stream stream, byte[] payload, CancellationToken cancellationToken = default)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var header = new byte[4];
            WriteInt32BigEndian(header, 0, payload.Length);
            await stream.WriteAsync(header, 0, header.Length, cancellationToken).ConfigureAwait(false);
            await stream.WriteAsync(payload, 0, payload.Length, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task<byte[]> ReadFrameAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var header = new byte[4];
            await ReadExactAsync(stream, header, cancellationToken).ConfigureAwait(false);
            int length = ReadInt32BigEndian(header, 0);
            if (length < 0 || length > 16 * 1024 * 1024)
            {
                throw new InvalidDataException($"Relay frame length out of range: {length}");
            }

            var payload = new byte[length];
            if (length > 0)
            {
                await ReadExactAsync(stream, payload, cancellationToken).ConfigureAwait(false);
            }

            return payload;
        }

        public static RelayEnvelope MakeSubmit(IReadOnlyList<GhostInput> inputs)
        {
            return new RelayEnvelope
            {
                Type = MessageSubmit,
                Inputs = GhostInputDto.FromDomain(inputs),
            };
        }

        public static RelayEnvelope MakeTape(ReplayTape tape)
        {
            return new RelayEnvelope
            {
                Type = MessageTape,
                Tape = ReplayTapeDto.FromDomain(tape),
            };
        }

        public static RelayEnvelope MakeError(string message)
        {
            return new RelayEnvelope
            {
                Type = MessageError,
                Message = message ?? "unknown error",
            };
        }

        private static async Task ReadExactAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
        {
            int offset = 0;
            while (offset < buffer.Length)
            {
                int read = await stream.ReadAsync(buffer, offset, buffer.Length - offset, cancellationToken)
                    .ConfigureAwait(false);
                if (read == 0)
                {
                    throw new EndOfStreamException("Relay connection closed mid-frame.");
                }

                offset += read;
            }
        }

        private static void WriteInt32BigEndian(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)value;
        }

        private static int ReadInt32BigEndian(byte[] buffer, int offset)
        {
            return (buffer[offset] << 24)
                | (buffer[offset + 1] << 16)
                | (buffer[offset + 2] << 8)
                | buffer[offset + 3];
        }
    }

    /// <summary>One TCP client session: connect, submit this side's inputs, wait for the authoritative tape.</summary>
    public sealed class RelayRoundClient : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private TcpClient _tcp;

        public RelayRoundClient(string host, int port)
        {
            _host = string.IsNullOrWhiteSpace(host) ? "127.0.0.1" : host;
            _port = port;
        }

        public async Task<ReplayTape> SubmitAndWaitAsync(
            IReadOnlyList<GhostInput> inputs,
            CancellationToken cancellationToken = default)
        {
            _tcp = new TcpClient();
            await _tcp.ConnectAsync(_host, _port).ConfigureAwait(false);
            NetworkStream stream = _tcp.GetStream();

            byte[] submit = RelayProtocol.SerializeEnvelope(RelayProtocol.MakeSubmit(inputs));
            await RelayProtocol.WriteFrameAsync(stream, submit, cancellationToken).ConfigureAwait(false);

            byte[] response = await RelayProtocol.ReadFrameAsync(stream, cancellationToken).ConfigureAwait(false);
            RelayEnvelope envelope = RelayProtocol.DeserializeEnvelope(response);

            if (envelope.Type == RelayProtocol.MessageError)
            {
                throw new InvalidOperationException("Relay error: " + envelope.Message);
            }

            if (envelope.Type != RelayProtocol.MessageTape || envelope.Tape == null)
            {
                throw new InvalidDataException("Expected Tape envelope from relay, got: " + envelope.Type);
            }

            return envelope.Tape.ToDomain();
        }

        /// <summary>Blocking helper for background threads (Unity coroutine poll loop).</summary>
        public ReplayTape SubmitAndWait(IReadOnlyList<GhostInput> inputs)
        {
            return SubmitAndWaitAsync(inputs).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            if (_tcp != null)
            {
                _tcp.Close();
                _tcp = null;
            }
        }
    }

    public sealed class RelayEnvelope
    {
        public string Type { get; set; }

        public GhostInputDto[] Inputs { get; set; }

        public ReplayTapeDto Tape { get; set; }

        public string Message { get; set; }
    }

    public sealed class GhostInputDto
    {
        public int PawnId { get; set; }

        public float StartX { get; set; }

        public float StartY { get; set; }

        public int StartFloor { get; set; }

        public ActionNodeDto[] Nodes { get; set; }

        public int StartingWounds { get; set; }

        public static GhostInputDto[] FromDomain(IReadOnlyList<GhostInput> inputs)
        {
            if (inputs == null || inputs.Count == 0)
            {
                return Array.Empty<GhostInputDto>();
            }

            var result = new GhostInputDto[inputs.Count];
            for (int i = 0; i < inputs.Count; i++)
            {
                GhostInput input = inputs[i];
                IReadOnlyList<ActionNode> nodes = input.Payload != null ? input.Payload.Nodes : Array.Empty<ActionNode>();
                var nodeDtos = new ActionNodeDto[nodes.Count];
                for (int n = 0; n < nodes.Count; n++)
                {
                    nodeDtos[n] = ActionNodeDto.FromDomain(nodes[n]);
                }

                result[i] = new GhostInputDto
                {
                    PawnId = input.PawnId,
                    StartX = input.Start.X,
                    StartY = input.Start.Y,
                    StartFloor = (int)input.Start.Floor,
                    Nodes = nodeDtos,
                    StartingWounds = input.StartingWounds,
                };
            }

            return result;
        }

        public GhostInput ToDomain()
        {
            var nodes = new List<ActionNode>();
            if (Nodes != null)
            {
                for (int i = 0; i < Nodes.Length; i++)
                {
                    nodes.Add(Nodes[i].ToDomain());
                }
            }

            return new GhostInput(
                PawnId,
                new PlanarPosition(StartX, StartY, (Floor)StartFloor),
                new TimelinePayload(nodes),
                StartingWounds);
        }
    }

    public sealed class ActionNodeDto
    {
        public int Verb { get; set; }

        public float ExecuteTime { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public int Floor { get; set; }

        public int Stance { get; set; }

        public int ShootMode { get; set; }

        public int Door { get; set; }

        public static ActionNodeDto FromDomain(ActionNode node)
        {
            return new ActionNodeDto
            {
                Verb = (int)node.Verb,
                ExecuteTime = node.ExecuteTime,
                X = node.Position.X,
                Y = node.Position.Y,
                Floor = (int)node.Position.Floor,
                Stance = (int)node.Stance,
                ShootMode = (int)node.ShootMode,
                Door = (int)node.Door,
            };
        }

        public ActionNode ToDomain()
        {
            // Modifier (CardData) is deferred gear — always null on the wire (C34).
            return new ActionNode(
                (ActionVerb)Verb,
                ExecuteTime,
                new PlanarPosition(X, Y, (Floor)Floor),
                (StanceType)Stance,
                modifier: null,
                shootMode: (ShootMode)ShootMode,
                doorAction: (DoorAction)Door);
        }
    }

    public sealed class ReplayTapeDto
    {
        public TrackDto[] Tracks { get; set; }

        public TapeEventDto[] Events { get; set; }

        public WoundDto[] EndWounds { get; set; }

        public static ReplayTapeDto FromDomain(ReplayTape tape)
        {
            if (tape == null)
            {
                throw new ArgumentNullException(nameof(tape));
            }

            var tracks = new List<TrackDto>();
            foreach (KeyValuePair<int, ScheduledPath> entry in tape.Tracks)
            {
                tracks.Add(TrackDto.FromDomain(entry.Key, entry.Value));
            }

            tracks.Sort((a, b) => a.PawnId.CompareTo(b.PawnId));

            var events = new TapeEventDto[tape.Events.Count];
            for (int i = 0; i < tape.Events.Count; i++)
            {
                events[i] = TapeEventDto.FromDomain(tape.Events[i]);
            }

            var wounds = new List<WoundDto>();
            foreach (KeyValuePair<int, int> entry in tape.EndWounds)
            {
                wounds.Add(new WoundDto { PawnId = entry.Key, Wounds = entry.Value });
            }

            wounds.Sort((a, b) => a.PawnId.CompareTo(b.PawnId));

            return new ReplayTapeDto
            {
                Tracks = tracks.ToArray(),
                Events = events,
                EndWounds = wounds.ToArray(),
            };
        }

        public ReplayTape ToDomain()
        {
            var tracks = new Dictionary<int, ScheduledPath>();
            if (Tracks != null)
            {
                for (int i = 0; i < Tracks.Length; i++)
                {
                    TrackDto track = Tracks[i];
                    tracks[track.PawnId] = track.ToDomain();
                }
            }

            var events = new List<TapeEvent>();
            if (Events != null)
            {
                for (int i = 0; i < Events.Length; i++)
                {
                    events.Add(Events[i].ToDomain());
                }
            }

            var endWounds = new Dictionary<int, int>();
            if (EndWounds != null)
            {
                for (int i = 0; i < EndWounds.Length; i++)
                {
                    endWounds[EndWounds[i].PawnId] = EndWounds[i].Wounds;
                }
            }

            return new ReplayTape(tracks, events, endWounds);
        }
    }

    public sealed class TrackDto
    {
        public int PawnId { get; set; }

        public float[] Xs { get; set; }

        public float[] Ys { get; set; }

        public int[] Floors { get; set; }

        public float[] ArrivalSeconds { get; set; }

        public static TrackDto FromDomain(int pawnId, ScheduledPath path)
        {
            int count = path != null ? path.Nodes.Count : 0;
            var xs = new float[count];
            var ys = new float[count];
            var floors = new int[count];
            var arrivals = new float[count];
            for (int i = 0; i < count; i++)
            {
                PlanarPosition p = path.Nodes[i];
                xs[i] = p.X;
                ys[i] = p.Y;
                floors[i] = (int)p.Floor;
                arrivals[i] = path.ArrivalSeconds[i];
            }

            return new TrackDto
            {
                PawnId = pawnId,
                Xs = xs,
                Ys = ys,
                Floors = floors,
                ArrivalSeconds = arrivals,
            };
        }

        public ScheduledPath ToDomain()
        {
            int count = Xs != null ? Xs.Length : 0;
            var waypoints = new List<PlanarPosition>(count);
            var arrivals = new List<float>(count);
            for (int i = 0; i < count; i++)
            {
                int floor = Floors != null && i < Floors.Length ? Floors[i] : 0;
                waypoints.Add(new PlanarPosition(Xs[i], Ys[i], (Floor)floor));
                arrivals.Add(ArrivalSeconds[i]);
            }

            return ScheduledPath.FromTimedWaypoints(waypoints, arrivals);
        }
    }

    public sealed class TapeEventDto
    {
        public float Seconds { get; set; }

        public int PawnId { get; set; }

        public int Type { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public int Floor { get; set; }

        public int TargetPawnId { get; set; }

        public float WindowStartSeconds { get; set; }

        public static TapeEventDto FromDomain(TapeEvent tapeEvent)
        {
            return new TapeEventDto
            {
                Seconds = tapeEvent.Seconds,
                PawnId = tapeEvent.PawnId,
                Type = (int)tapeEvent.Type,
                X = tapeEvent.Position.X,
                Y = tapeEvent.Position.Y,
                Floor = (int)tapeEvent.Position.Floor,
                TargetPawnId = tapeEvent.TargetPawnId,
                WindowStartSeconds = tapeEvent.WindowStartSeconds,
            };
        }

        public TapeEvent ToDomain()
        {
            return new TapeEvent(
                Seconds,
                PawnId,
                (TapeEventType)Type,
                new PlanarPosition(X, Y, (Floor)Floor),
                TargetPawnId,
                WindowStartSeconds);
        }
    }

    public sealed class WoundDto
    {
        public int PawnId { get; set; }

        public int Wounds { get; set; }
    }
}
