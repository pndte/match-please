using System;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.UseCases.Match;
using Bw.UseCases.Match.View.Adapters;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Match.View.Adapters
{
    public sealed class MatchCountdownTests
    {
        private MatchBoard _board;
        private ServerTime _serverTime;
        private MatchCountdown _countdown;

        [SetUp]
        public void SetUp()
        {
            _board = new MatchBoard();
            _serverTime = new ServerTime();
            _countdown = new MatchCountdown(_board, _serverTime, new ManualClock(0.25f));
            _board.Apply(new MatchStandings(new MatchPeriod(MatchPhase.Running, 100), Array.Empty<Standing>()));
        }

        [TestCase(60, 10)]
        [TestCase(61, 10)]
        [TestCase(63, 10)]
        [TestCase(64, 9)]
        [TestCase(99, 1)]
        [TestCase(100, 0)]
        [TestCase(130, 0)]
        public void SecondsLeftAreTheTicksToTheEndCeiled(int tick, int seconds)
        {
            _serverTime.Tick = tick;

            Assert.That(_countdown.SecondsLeft, Is.EqualTo(seconds));
        }

        [Test]
        public void CountdownFollowsTheBoardsPeriod()
        {
            _serverTime.Tick = 100;

            _board.Apply(new MatchStandings(new MatchPeriod(MatchPhase.Ended, 140), Array.Empty<Standing>()));

            Assert.That(_countdown.SecondsLeft, Is.EqualTo(10));
        }

        private sealed class ServerTime : IServerTime
        {
            public int Tick { get; set; }
        }
    }
}
