using System.Linq;
using System.Text.RegularExpressions;
using Bw.CoreAdapters.Characters;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.PropsMatchBots.Doubles;
using Bw.UseCases.Character;
using Bw.UseCases.Match;
using Bw.UseCases.Match.Network;
using Bw.UseCases.Match.Network.Requests;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;

namespace Bw.Tests.UseCases.Match.Network
{
    public sealed class MatchReceiverTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private CharacterDescription _sanya;
        private CharacterDescription _seva;
        private CharacterCatalog _catalog;
        private LocalPlayer _localPlayer;
        private MatchBoard _board;
        private INetProperty<MatchTable> _table;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _sanya = ScriptableObject.CreateInstance<CharacterDescription>().With("_key", 7);
            _seva = ScriptableObject.CreateInstance<CharacterDescription>().With("_key", 9);
            _catalog = ScriptableObject.CreateInstance<CharacterCatalog>();
            _catalog.Characters = new[] { new CharacterEntry { Character = _sanya }, new CharacterEntry { Character = _seva } };
            _localPlayer = new LocalPlayer();
            _board = new MatchBoard();
            var schema = new NetEntriesSchemaBuilder();
            var declaration = schema.DeclareProperty(MatchTable.Empty, NetworkDelivery.ReliableFragmentedSequenced, NetworkPermissions.Everyone);
            var entries = UnspawnedNetEntries.Create(_lifetime.Lifetime, _objects.Create("Match"), schema.Build());
            _table = entries.Get(declaration);
            _ = new MatchReceiver(_lifetime.Lifetime, entries, declaration, _board, _catalog, _localPlayer);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
            Object.DestroyImmediate(_catalog);
            Object.DestroyImmediate(_sanya);
            Object.DestroyImmediate(_seva);
        }

        [Test]
        public void TableAtHandIsShownAtOnce()
        {
            Assert.That(_board.Current.Value.Period, Is.EqualTo(MatchPeriod.Starting));
            Assert.That(_board.Current.Value.Lines, Is.Empty);
        }

        [Test]
        public void ReceivedTableIsShownRankedWithItsPeriod()
        {
            _table.Value = Table(Entry(1, "Сева", 9, 1, 0), Entry(2, "Гошак", 7, 4, 2), Entry(3, "Саня", 7, 1, 3));

            var standings = _board.Current.Value;

            Assert.That(standings.Period, Is.EqualTo(new MatchPeriod(MatchPhase.Running, 900)));
            Assert.That(standings.Lines.Select(line => line.Nickname), Is.EqualTo(new[] { "Гошак", "Сева", "Саня" }));
        }

        [Test]
        public void EntryShowsItsCatalogCharacterAndScore()
        {
            _table.Value = Table(Entry(1, "Сева", 9, 2, 1));

            var line = _board.Current.Value.Lines[0];

            Assert.That(line.Character, Is.SameAs(_seva));
            Assert.That(line.Score.Kills, Is.EqualTo(2));
            Assert.That(line.Score.Deaths, Is.EqualTo(1));
        }

        [Test]
        public void NoLineIsMineWhileTheLocalPlayerIsUnknown()
        {
            _table.Value = Table(Entry(1, "Сева", 9, 0, 0), Entry(2, "Гошак", 7, 0, 0));

            Assert.That(_board.Current.Value.Lines.Any(line => line.IsMine), Is.False);
        }

        [Test]
        public void LocalPlayerArrivingLaterMarksItsLineInTheShownTable()
        {
            _table.Value = Table(Entry(1, "Сева", 9, 0, 0), Entry(2, "Гошак", 7, 0, 0));

            _localPlayer.Set(_lifetime.Lifetime, 2);

            Assert.That(_board.Current.Value.Lines.Single(line => line.IsMine).Nickname, Is.EqualTo("Гошак"));
        }

        [Test]
        public void LaterTablesKeepMarkingTheLocalPlayer()
        {
            _localPlayer.Set(_lifetime.Lifetime, 1);

            _table.Value = Table(Entry(1, "Сева", 9, 3, 0), Entry(2, "Гошак", 7, 0, 0));

            Assert.That(_board.Current.Value.Lines.Single(line => line.IsMine).Nickname, Is.EqualTo("Сева"));
        }

        [Test]
        public void EntryOfACharacterOutsideTheCatalogIsRejected()
        {
            LogAssert.Expect(LogType.Exception, new Regex("no character with key 5"));

            _table.Value = Table(Entry(1, "Сева", 5, 0, 0));

            Assert.That(_board.Current.Value.Lines, Is.Empty);
        }

        private static MatchTable Table(params MatchEntry[] entries) =>
            new(new MatchPeriod(MatchPhase.Running, 900), entries);

        private static MatchEntry Entry(
            int id,
            string nickname,
            int character,
            int kills,
            int deaths) =>
            new(new PlayerTag(id, nickname), character, new Score(kills, deaths, 0));
    }
}
