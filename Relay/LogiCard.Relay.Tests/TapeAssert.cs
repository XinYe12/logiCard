using System.Collections.Generic;
using LogiCard.Net;
using LogiCard.Sim;
using Xunit;

namespace LogiCard.Relay.Tests
{
    internal static class TapeAssert
    {
        public static void Equal(ReplayTape expected, ReplayTape actual)
        {
            Assert.NotNull(expected);
            Assert.NotNull(actual);
            Assert.Equal(expected.EndSeconds, actual.EndSeconds, precision: 4);
            Assert.Equal(expected.Events.Count, actual.Events.Count);
            for (int i = 0; i < expected.Events.Count; i++)
            {
                TapeEvent e = expected.Events[i];
                TapeEvent a = actual.Events[i];
                Assert.Equal(e.Seconds, a.Seconds, precision: 4);
                Assert.Equal(e.PawnId, a.PawnId);
                Assert.Equal(e.Type, a.Type);
                Assert.Equal(e.TargetPawnId, a.TargetPawnId);
                Assert.Equal(e.WindowStartSeconds, a.WindowStartSeconds, precision: 4);
                Assert.Equal(e.Position.X, a.Position.X, precision: 4);
                Assert.Equal(e.Position.Y, a.Position.Y, precision: 4);
                Assert.Equal(e.Position.Floor, a.Position.Floor);
            }

            Assert.Equal(expected.Tracks.Count, actual.Tracks.Count);
            foreach (KeyValuePair<int, ScheduledPath> entry in expected.Tracks)
            {
                Assert.True(actual.Tracks.ContainsKey(entry.Key), $"missing track for pawn {entry.Key}");
                ScheduledPath exp = entry.Value;
                ScheduledPath act = actual.Tracks[entry.Key];
                Assert.Equal(exp.Nodes.Count, act.Nodes.Count);
                for (int i = 0; i < exp.Nodes.Count; i++)
                {
                    Assert.Equal(exp.Nodes[i].X, act.Nodes[i].X, precision: 4);
                    Assert.Equal(exp.Nodes[i].Y, act.Nodes[i].Y, precision: 4);
                    Assert.Equal(exp.ArrivalSeconds[i], act.ArrivalSeconds[i], precision: 4);
                }
            }

            Assert.Equal(expected.EndWounds.Count, actual.EndWounds.Count);
            foreach (KeyValuePair<int, int> entry in expected.EndWounds)
            {
                Assert.Equal(entry.Value, actual.WoundsFor(entry.Key));
            }
        }
    }
}
