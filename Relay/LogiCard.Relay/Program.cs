using System;
using LogiCard.Net;
using LogiCard.Sim;

namespace LogiCard.Relay
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            int port = RelayProtocol.DefaultPort;
            string boardName = "demo";

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "--port" && i + 1 < args.Length && int.TryParse(args[++i], out int parsed))
                {
                    port = parsed;
                }
                else if (arg == "--board" && i + 1 < args.Length)
                {
                    boardName = args[++i];
                }
                else if (arg == "--help" || arg == "-h")
                {
                    PrintUsage();
                    return 0;
                }
            }

            ArenaBoard board;
            switch (boardName.ToLowerInvariant())
            {
                case "demo":
                    board = DemoArenaBoard.CreateDemo();
                    break;
                case "empty":
                    board = DemoArenaBoard.CreateEmpty();
                    break;
                default:
                    Console.Error.WriteLine($"Unknown board '{boardName}'. Use demo|empty.");
                    return 2;
            }

            using var server = new RelayServer(port, board);
            server.Start();
            Console.WriteLine($"[relay] board={boardName} — waiting for exactly two clients, then resolving one round.");
            try
            {
                server.Completion.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[relay] failed: " + ex);
                return 1;
            }

            return 0;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("LogiCard.Relay — Phase 2 first-slice resolve relay (C52)");
            Console.WriteLine("  --port N       listen port (default 7777)");
            Console.WriteLine("  --board name   demo (GameBootstrap layout) | empty (default bounds, no walls)");
        }
    }
}
