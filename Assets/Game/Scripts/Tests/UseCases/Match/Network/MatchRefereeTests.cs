using System.Collections.Generic;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Players;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.PropsMatchBots.Doubles;
using Bw.UseCases.Match;
using Bw.UseCases.Match.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Match.Network
{
    public sealed class MatchRefereeTests
    {
        private LifetimeDefinition _lifetime;
        private ManualWorldStep _worldStep;
        private MatchConfig _config;
        private PlayerCollection _players;
        private Scoreboard _scoreboard;
        private MatchReferee _referee;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _worldStep = new ManualWorldStep();
            _config = ScriptableObject.CreateInstance<MatchConfig>();
            _config.MatchSeconds = 2f;
            _config.ResultSeconds = 1f;
            _players = new PlayerCollection();
            _scoreboard = new Scoreboard(_lifetime.Lifetime, _players);
            _referee = new MatchReferee(_lifetime.Lifetime, new ManualClock(0.25f), _worldStep, _config, _scoreboard);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void MatchWaitsToStart()
        {
            Assert.That(_referee.Period.Value, Is.EqualTo(MatchPeriod.Starting));
        }

        [Test]
        public void FirstLateTickStartsTheMatchForItsLength()
        {
            Late(5);

            Assert.That(_referee.Period.Value, Is.EqualTo(new MatchPeriod(MatchPhase.Running, 13)));
        }

        [Test]
        public void OtherPhasesDoNotMoveTheMatch()
        {
            _worldStep.Fire(TickPhase.Default, 5);
            _worldStep.Fire(TickPhase.Publish, 5);

            Assert.That(_referee.Period.Value, Is.EqualTo(MatchPeriod.Starting));
        }

        [Test]
        public void TickBeforeTheStartingEndWaits()
        {
            Late(-3);

            Assert.That(_referee.Period.Value, Is.EqualTo(MatchPeriod.Starting));
        }

        [Test]
        public void RunningMatchGoesOnUntilItsEndTick()
        {
            Late(0);

            Late(7);

            Assert.That(_referee.Period.Value.Phase, Is.EqualTo(MatchPhase.Running));
        }

        [Test]
        public void MatchEndsAtItsEndTickAndShowsTheResults()
        {
            Late(0);

            Late(8);

            Assert.That(_referee.Period.Value, Is.EqualTo(new MatchPeriod(MatchPhase.Ended, 12)));
        }

        [Test]
        public void NewMatchStartsWhenTheResultsAreOver()
        {
            Late(0);
            Late(8);

            Late(12);

            Assert.That(_referee.Period.Value, Is.EqualTo(new MatchPeriod(MatchPhase.Running, 20)));
        }

        [Test]
        public void LateTickAfterTheEndStillEndsTheMatch()
        {
            Late(0);

            Late(30);

            Assert.That(_referee.Period.Value, Is.EqualTo(new MatchPeriod(MatchPhase.Ended, 34)));
        }

        [Test]
        public void EveryStartZeroesTheScores()
        {
            var player = new Player("Сева");
            _players.AddLifetimed(_lifetime.Lifetime, player);
            Late(0);
            _scoreboard.Credit(player, new Score(2, 1, 40));
            Late(8);

            Late(12);

            Assert.That(_scoreboard.Scores[player].Kills, Is.Zero);
            Assert.That(_scoreboard.Scores[player].Damage, Is.Zero);
        }

        [Test]
        public void EndKeepsTheScoresForTheResults()
        {
            var player = new Player("Сева");
            _players.AddLifetimed(_lifetime.Lifetime, player);
            Late(0);
            _scoreboard.Credit(player, new Score(2, 1, 40));

            Late(8);

            Assert.That(_scoreboard.Scores[player].Kills, Is.EqualTo(2));
        }

        [Test]
        public void PeriodChangesOnlyAtItsBoundaries()
        {
            var periods = new List<MatchPeriod>();
            _referee.Period.Advise(_lifetime.Lifetime, period => periods.Add(period));

            for (var tick = 0; tick <= 12; tick++)
                Late(tick);

            Assert.That(periods, Is.EqualTo(new[]
            {
                MatchPeriod.Starting,
                new MatchPeriod(MatchPhase.Running, 8),
                new MatchPeriod(MatchPhase.Ended, 12),
                new MatchPeriod(MatchPhase.Running, 20)
            }));
        }

        private void Late(int tick) =>
            _worldStep.Fire(TickPhase.Late, tick);
    }
}
