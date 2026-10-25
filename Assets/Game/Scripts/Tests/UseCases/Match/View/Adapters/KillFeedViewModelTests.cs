using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bw.UseCases.Combat.Weapon;
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
    public sealed class KillFeedViewModelTests
    {
        private LifetimeDefinition _lifetime;
        private KillFeed _feed;
        private WeaponDescription _pistol;
        private WeaponDescription _shotgun;
        private WeaponDescription _sniper;
        private Texture2D _texture;
        private Sprite _pistolIcon;
        private Sprite _shotgunIcon;
        private WeaponIconsConfig _icons;
        private List<KillLine> _killed;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _feed = new KillFeed();
            _pistol = ScriptableObject.CreateInstance<WeaponDescription>();
            _shotgun = ScriptableObject.CreateInstance<WeaponDescription>();
            _sniper = ScriptableObject.CreateInstance<WeaponDescription>();
            _texture = new Texture2D(2, 2);
            _pistolIcon = Sprite.Create(_texture, new Rect(0f, 0f, 1f, 1f), Vector2.zero);
            _shotgunIcon = Sprite.Create(_texture, new Rect(1f, 1f, 1f, 1f), Vector2.zero);
            _icons = ScriptableObject.CreateInstance<WeaponIconsConfig>();
            _icons.Icons = new[]
            {
                new WeaponIcon { Weapon = _pistol, Icon = _pistolIcon },
                new WeaponIcon { Weapon = _shotgun, Icon = _shotgunIcon }
            };
            _killed = new List<KillLine>();
            var viewModel = new KillFeedViewModel(_lifetime.Lifetime, _feed, _icons);
            viewModel.Killed.Advise(_lifetime.Lifetime, line => _killed.Add(line));
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            Object.DestroyImmediate(_icons);
            Object.DestroyImmediate(_pistolIcon);
            Object.DestroyImmediate(_shotgunIcon);
            Object.DestroyImmediate(_texture);
            Object.DestroyImmediate(_pistol);
            Object.DestroyImmediate(_shotgun);
            Object.DestroyImmediate(_sniper);
        }

        [Test]
        public void KillBecomesALineWithBothNamesAndTheWeaponsIcon()
        {
            _feed.Report(new KillNote(new KillParty("Сева", true), new KillParty("Гошак", false), _shotgun));

            Assert.That(_killed, Has.Count.EqualTo(1));
            Assert.That(_killed[0].Killer.Nickname, Is.EqualTo("Сева"));
            Assert.That(_killed[0].Killer.Mine, Is.True);
            Assert.That(_killed[0].Victim.Nickname, Is.EqualTo("Гошак"));
            Assert.That(_killed[0].Victim.Mine, Is.False);
            Assert.That(_killed[0].WeaponIcon, Is.SameAs(_shotgunIcon));
        }

        [Test]
        public void KillWithAWeaponWithoutAnIconIsRejected()
        {
            LogAssert.Expect(LogType.Exception, new Regex("has no icon for weapon"));

            _feed.Report(new KillNote(new KillParty("Сева", false), new KillParty("Гошак", false), _sniper));

            Assert.That(_killed, Is.Empty);
        }

        [Test]
        public void IconOfAListedWeaponIsItsRow()
        {
            Assert.That(_icons.IconOf(_pistol), Is.SameAs(_pistolIcon));
            Assert.That(_icons.IconOf(_shotgun), Is.SameAs(_shotgunIcon));
        }

        [Test]
        public void IconOfAnUnlistedWeaponThrows()
        {
            Assert.Throws<InvalidOperationException>(() => _icons.IconOf(_sniper));
        }
    }
}
