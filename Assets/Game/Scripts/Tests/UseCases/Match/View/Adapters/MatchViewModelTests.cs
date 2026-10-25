using System;
using System.Text.RegularExpressions;
using Bw.CoreAdapters.Characters;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.PropsMatchBots.Doubles;
using Bw.UseCases.Character;
using Bw.UseCases.Match;
using Bw.UseCases.Match.View.Adapters;
using Bw.UseCases.Match.View.Hud;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Match.View.Adapters
{
    public sealed class MatchViewModelTests
    {
        private LifetimeDefinition _lifetime;
        private StubNetworkObject _matchObject;
        private MatchBoard _board;
        private CharacterDescription _sanya;
        private CharacterDescription _stranger;
        private Texture2D _texture;
        private Sprite _portrait;
        private CharacterLooksConfig _looks;
        private MatchViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _matchObject = new StubNetworkObject(_lifetime.Lifetime);
            _board = new MatchBoard();
            _sanya = ScriptableObject.CreateInstance<CharacterDescription>();
            _stranger = ScriptableObject.CreateInstance<CharacterDescription>();
            _texture = new Texture2D(2, 2);
            _portrait = Sprite.Create(_texture, new Rect(0f, 0f, 2f, 2f), Vector2.zero);
            _looks = ScriptableObject.CreateInstance<CharacterLooksConfig>();
            _looks.Looks = new[] { new CharacterLook { Character = _sanya, Portrait = _portrait } };
            var countdown = new MatchCountdown(_board, new ServerTime(), new ManualClock(0.5f));
            _viewModel = new MatchViewModel(_lifetime.Lifetime, _matchObject, _board, countdown, _looks);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            Object.DestroyImmediate(_looks);
            Object.DestroyImmediate(_portrait);
            Object.DestroyImmediate(_texture);
            Object.DestroyImmediate(_sanya);
            Object.DestroyImmediate(_stranger);
        }

        [Test]
        public void HudIsShownOnlyWhileTheMatchIsSpawned()
        {
            Assert.That(_viewModel.Shown.Value, Is.False);

            _matchObject.Spawn();
            var spawned = _viewModel.Shown.Value;
            _matchObject.Despawn();

            Assert.That(spawned, Is.True);
            Assert.That(_viewModel.Shown.Value, Is.False);
        }

        [Test]
        public void EmptyBoardIsAnEmptySheetOfTheStartingStage()
        {
            Assert.That(_viewModel.Sheet.Value.Stage, Is.EqualTo(MatchStage.Starting));
            Assert.That(_viewModel.Sheet.Value.Lines, Is.Empty);
        }

        [TestCase(MatchPhase.Starting, MatchStage.Starting)]
        [TestCase(MatchPhase.Running, MatchStage.Running)]
        [TestCase(MatchPhase.Ended, MatchStage.Ended)]
        public void EveryPhaseHasItsStage(MatchPhase phase, MatchStage stage)
        {
            _board.Apply(new MatchStandings(new MatchPeriod(phase, 0), Array.Empty<Standing>()));

            Assert.That(_viewModel.Sheet.Value.Stage, Is.EqualTo(stage));
        }

        [Test]
        public void StandingBecomesALineWithItsPortraitAndStats()
        {
            _board.Apply(Standings(new Standing("Саня", _sanya, new Score(3, 2, 150), true)));

            var line = _viewModel.Sheet.Value.Lines[0];

            Assert.That(line.Nickname, Is.EqualTo("Саня"));
            Assert.That(line.Portrait, Is.SameAs(_portrait));
            Assert.That(line.Mine, Is.True);
            Assert.That(line.Stats.Kills, Is.EqualTo(3));
            Assert.That(line.Stats.Deaths, Is.EqualTo(2));
            Assert.That(line.Stats.Damage, Is.EqualTo(150));
            Assert.That(line.Stats.KillsPerDeath, Is.EqualTo(1.5f).Within(1e-6f));
            Assert.That(line.Stats.DamagePerKill, Is.EqualTo(50));
        }

        [Test]
        public void LineWithoutKillsHasNoDamagePerKill()
        {
            _board.Apply(Standings(new Standing("Саня", _sanya, new Score(0, 4, 80), false)));

            Assert.That(_viewModel.Sheet.Value.Lines[0].Stats.DamagePerKill, Is.Zero);
            Assert.That(_viewModel.Sheet.Value.Lines[0].Stats.KillsPerDeath, Is.Zero);
        }

        [Test]
        public void LinesKeepTheBoardsOrder()
        {
            _board.Apply(Standings(
                new Standing("Первый", _sanya, new Score(1, 0, 0), false),
                new Standing("Второй", _sanya, new Score(5, 0, 0), false)));

            Assert.That(_viewModel.Sheet.Value.Lines[0].Nickname, Is.EqualTo("Первый"));
            Assert.That(_viewModel.Sheet.Value.Lines[1].Nickname, Is.EqualTo("Второй"));
        }

        [Test]
        public void SecondsLeftComeFromTheCountdown()
        {
            _board.Apply(new MatchStandings(new MatchPeriod(MatchPhase.Running, 20), Array.Empty<Standing>()));

            Assert.That(_viewModel.SecondsLeft, Is.EqualTo(10));
        }

        [Test]
        public void CharacterWithoutALookIsRejected()
        {
            LogAssert.Expect(LogType.Exception, new Regex("has no look for character"));

            _board.Apply(Standings(new Standing("Чужой", _stranger, default, false)));

            Assert.That(_viewModel.Sheet.Value.Lines, Is.Empty);
        }

        [Test]
        public void UnknownPhaseIsRejected()
        {
            LogAssert.Expect(LogType.Exception, new Regex("phase"));

            _board.Apply(new MatchStandings(new MatchPeriod((MatchPhase)9, 0), Array.Empty<Standing>()));

            Assert.That(_viewModel.Sheet.Value.Stage, Is.EqualTo(MatchStage.Starting));
        }

        private static MatchStandings Standings(params Standing[] lines) =>
            new(new MatchPeriod(MatchPhase.Running, 100), lines);

        private sealed class ServerTime : IServerTime
        {
            public int Tick => 0;
        }
    }
}
