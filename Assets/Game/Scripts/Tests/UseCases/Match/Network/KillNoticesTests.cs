using System;
using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.PropsMatchBots.Doubles;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Match.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Match.Network
{
    public sealed class KillNoticesTests
    {
        private LifetimeDefinition _lifetime;
        private PlayerIds _ids;
        private WeaponDescriptions _descriptions;
        private WeaponDescription _shotgun;
        private Player _killer;
        private Player _victim;
        private StubWeapon _weapon;
        private KillNotices _notices;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _ids = new PlayerIds();
            _descriptions = new WeaponDescriptions();
            _shotgun = ScriptableObject.CreateInstance<WeaponDescription>().With("_key", 42);
            _killer = new Player("Сева");
            _victim = new Player("Гошак");
            _weapon = new StubWeapon();
            _ids.AddLifetimed(_lifetime.Lifetime, _killer);
            _ids.AddLifetimed(_lifetime.Lifetime, _victim);
            _descriptions.Issue(_weapon, _shotgun);
            _notices = new KillNotices(_ids, _descriptions);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            Object.DestroyImmediate(_shotgun);
        }

        [Test]
        public void NoticeNamesBothPlayersByIdAndNickname()
        {
            var notice = _notices.Of(new Kill(_killer, _victim, _weapon));

            Assert.That(notice.Killer.Id, Is.EqualTo(1));
            Assert.That(notice.Killer.Nickname, Is.EqualTo("Сева"));
            Assert.That(notice.Victim.Id, Is.EqualTo(2));
            Assert.That(notice.Victim.Nickname, Is.EqualTo("Гошак"));
        }

        [Test]
        public void NoticeCarriesTheWeaponsContentKey()
        {
            var notice = _notices.Of(new Kill(_killer, _victim, _weapon));

            Assert.That(notice.WeaponKey, Is.EqualTo(42));
        }

        [Test]
        public void PlayerWithoutAnIdThrows()
        {
            Assert.Throws<InvalidOperationException>(() => _notices.Of(new Kill(new Player("Чужой"), _victim, _weapon)));
        }
    }
}
