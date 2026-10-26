using System.Collections.Generic;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.View;
using Bw.UseCases.Combat.View.Adapters.Hit;
using Bw.UseCases.Combat.Weapon.Network.Prediction;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.View.Adapters.Hit
{
    public sealed class ArrivingStrikesTests
    {
        private LifetimeDefinition _lifetime;
        private FakeStrikes _strikes;
        private ShotLaunches _launches;
        private ShotVfxConfig _vfx;
        private ArrivingStrikes _arriving;
        private List<ShotLaunch> _launched;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _strikes = new FakeStrikes();
            _launches = new ShotLaunches();
            _vfx = ScriptableObject.CreateInstance<ShotVfxConfig>();
            _vfx.BulletSpeed = 10f;
            _launched = new List<ShotLaunch>();
            _launches.Launched.Advise(_lifetime.Lifetime, _launched.Add);
            _arriving = new ArrivingStrikes(_lifetime.Lifetime, _strikes, _launches, _vfx);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            Object.DestroyImmediate(_vfx);
        }

        [Test]
        public void EveryTargetIsLaunchedAtFromTheActorsHands()
        {
            var actor = new FakeCharacter();
            actor.Hands.MoveTo(new Vector2(-2f, 1f));
            var first = new FakeMortal();
            var second = new FakeMortal();

            _strikes.Strike(FakeStrikes.Of(10, actor, first, second));

            Assert.That(_launched.Count, Is.EqualTo(2));
            Assert.That(_launched[0].Target, Is.SameAs(first));
            Assert.That(_launched[1].Target, Is.SameAs(second));
            Assert.That(_launched[0].From, Is.EqualTo(new Vector2(-2f, 1f)));
            Assert.That(_launched[0].Speed, Is.EqualTo(10f));
        }

        [Test]
        public void LaunchesComeBeforeTheStrikeIsPassedOn()
        {
            var heard = -1;
            _arriving.Struck.Advise(_lifetime.Lifetime, _ => heard = _launched.Count);

            _strikes.Strike(FakeStrikes.Of(10, new FakeCharacter(), new FakeMortal()));

            Assert.That(heard, Is.EqualTo(1));
        }

        [Test]
        public void StrikeIsPassedOnAsItCame()
        {
            var strike = FakeStrikes.Of(7, new FakeCharacter(), new FakeMortal());
            var passed = new List<Strike>();
            _arriving.Struck.Advise(_lifetime.Lifetime, passed.Add);

            _strikes.Strike(strike);

            Assert.That(passed.Count, Is.EqualTo(1));
            Assert.That(passed[0].Tick, Is.EqualTo(7));
            Assert.That(passed[0].Affected, Is.SameAs(strike.Affected));
        }
    }
}
