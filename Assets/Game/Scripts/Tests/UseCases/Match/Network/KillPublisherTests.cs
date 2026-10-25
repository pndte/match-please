using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.PropsMatchBots.Doubles;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Match.Network;
using Bw.UseCases.Match.Network.Requests;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Match.Network
{
    public sealed class KillPublisherTests
    {
        private LifetimeDefinition _lifetime;
        private Kills _kills;
        private RecordingResults<KillNotice> _sender;
        private WeaponDescription _rifle;
        private Player _killer;
        private Player _victim;
        private StubWeapon _weapon;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _kills = new Kills();
            _sender = new RecordingResults<KillNotice>();
            _rifle = ScriptableObject.CreateInstance<WeaponDescription>().With("_key", 5);
            _killer = new Player("Сева");
            _victim = new Player("Гошак");
            _weapon = new StubWeapon();
            var ids = new PlayerIds();
            ids.AddLifetimed(_lifetime.Lifetime, _killer);
            ids.AddLifetimed(_lifetime.Lifetime, _victim);
            var descriptions = new WeaponDescriptions();
            descriptions.Issue(_weapon, _rifle);
            _ = new KillPublisher(_lifetime.Lifetime, _kills, new KillNotices(ids, descriptions), _sender);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            Object.DestroyImmediate(_rifle);
        }

        [Test]
        public void EveryScoredKillIsBroadcastAsANotice()
        {
            _kills.Score(new Kill(_killer, _victim, _weapon));
            _kills.Score(new Kill(_victim, _killer, _weapon));

            Assert.That(_sender.Broadcasts, Has.Count.EqualTo(2));
            Assert.That(_sender.Broadcasts[0].Killer.Nickname, Is.EqualTo("Сева"));
            Assert.That(_sender.Broadcasts[0].Victim.Nickname, Is.EqualTo("Гошак"));
            Assert.That(_sender.Broadcasts[1].Killer.Nickname, Is.EqualTo("Гошак"));
            Assert.That(_sender.Broadcasts[0].WeaponKey, Is.EqualTo(5));
            Assert.That(_sender.Sent, Is.Empty);
        }

        [Test]
        public void NothingIsBroadcastAfterTheLifetime()
        {
            _lifetime.Terminate();

            _kills.Score(new Kill(_killer, _victim, _weapon));

            Assert.That(_sender.Broadcasts, Is.Empty);
        }

        private sealed class Kills : IKills
        {
            public ISource<Kill> Scored => _scored;

            private readonly Signal<Kill> _scored = new();

            public void Score(Kill kill) =>
                _scored.Fire(kill);
        }
    }
}
