using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bw.CoreAdapters.Weapons;
using Bw.Entities.Network;
using Bw.Tests.Doubles;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Match;
using Bw.UseCases.Match.Network;
using Bw.UseCases.Match.Network.Requests;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Bw.Tests.UseCases.Match.Network
{
    public sealed class KillReceiverTests
    {
        private LifetimeDefinition _lifetime;
        private LocalPlayer _localPlayer;
        private ManualResults<KillNotice> _notices;
        private KillFeed _feed;
        private WeaponCatalog _catalog;
        private WeaponDescription _pistol;
        private List<KillNote> _reported;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _localPlayer = new LocalPlayer();
            _notices = new ManualResults<KillNotice>();
            _feed = new KillFeed();
            _pistol = ScriptableObject.CreateInstance<WeaponDescription>().With("_key", 3);
            _catalog = ScriptableObject.CreateInstance<WeaponCatalog>();
            _catalog.Weapons = new[] { new WeaponEntry { Weapon = _pistol } };
            _reported = new List<KillNote>();
            _feed.Reported.Advise(_lifetime.Lifetime, kill => _reported.Add(kill));
            _ = new KillReceiver(_lifetime.Lifetime, _localPlayer, _notices, _feed, _catalog);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            Object.DestroyImmediate(_catalog);
            Object.DestroyImmediate(_pistol);
        }

        [Test]
        public void NoticeIsReportedToTheFeedWithItsWeapon()
        {
            _notices.Deliver(Notice(1, 2, 3));

            Assert.That(_reported, Has.Count.EqualTo(1));
            Assert.That(_reported[0].Killer.Nickname, Is.EqualTo("Сева"));
            Assert.That(_reported[0].Victim.Nickname, Is.EqualTo("Гошак"));
            Assert.That(_reported[0].Weapon, Is.SameAs(_pistol));
        }

        [Test]
        public void NobodyIsMineWhileTheLocalPlayerIsUnknown()
        {
            _notices.Deliver(Notice(1, 2, 3));

            Assert.That(_reported[0].Killer.IsMine, Is.False);
            Assert.That(_reported[0].Victim.IsMine, Is.False);
        }

        [Test]
        public void LocalPlayersKillIsMine()
        {
            _localPlayer.Set(_lifetime.Lifetime, 1);

            _notices.Deliver(Notice(1, 2, 3));

            Assert.That(_reported[0].Killer.IsMine, Is.True);
            Assert.That(_reported[0].Victim.IsMine, Is.False);
        }

        [Test]
        public void LocalPlayersDeathIsMine()
        {
            _localPlayer.Set(_lifetime.Lifetime, 2);

            _notices.Deliver(Notice(1, 2, 3));

            Assert.That(_reported[0].Killer.IsMine, Is.False);
            Assert.That(_reported[0].Victim.IsMine, Is.True);
        }

        [Test]
        public void NoticeOfAWeaponOutsideTheCatalogIsRejected()
        {
            LogAssert.Expect(LogType.Exception, new Regex("no weapon with key 99"));

            _notices.Deliver(Notice(1, 2, 99));

            Assert.That(_reported, Is.Empty);
        }

        private static KillNotice Notice(int killer, int victim, int weapon) =>
            new(new PlayerTag(killer, "Сева"), new PlayerTag(victim, "Гошак"), weapon);
    }
}
