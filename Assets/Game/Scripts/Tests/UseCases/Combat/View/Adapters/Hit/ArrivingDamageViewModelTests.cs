using Bw.Entities.Physics;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.View.Adapters.Hit;
using Bw.UseCases.Mortality.View.Hit;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.View.Adapters.Hit
{
    public sealed class ArrivingDamageViewModelTests
    {
        private LifetimeDefinition _lifetime;
        private Damage _damage;
        private FakeMortal _mortal;
        private ShotLaunches _launches;
        private GameObject _body;
        private int _taken;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _damage = new Damage();
            _mortal = new FakeMortal();
            _launches = new ShotLaunches();
            _body = new GameObject("Body");
            _taken = 0;
            new ArrivingDamageViewModel(_lifetime.Lifetime, _damage, _mortal, _launches, _body.AddComponent<BodyBox>())
                .Taken.Advise(_lifetime.Lifetime, _ => _taken++);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            Object.DestroyImmediate(_body);
        }

        [Test]
        public void DamageWithNothingLaunchedIsTakenAtOnce()
        {
            _damage.Take();

            Assert.That(_taken, Is.EqualTo(1));
        }

        [Test]
        public void DamageWaitsForTheBulletLaunchedFromAfar()
        {
            _launches.Launch(new ShotLaunch(_mortal, new Vector2(20f, 0f), 80f));

            _damage.Take();

            Assert.That(_taken, Is.Zero);
        }

        [Test]
        public void BulletLaunchedPointBlankLandsAtOnce()
        {
            _launches.Launch(new ShotLaunch(_mortal, Vector2.zero, 80f));

            _damage.Take();

            Assert.That(_taken, Is.EqualTo(1));
        }

        [Test]
        public void BulletAtAnotherMortalHoldsNothingBack()
        {
            _launches.Launch(new ShotLaunch(new FakeMortal(), new Vector2(20f, 0f), 80f));

            _damage.Take();

            Assert.That(_taken, Is.EqualTo(1));
        }

        private sealed class Damage : IDamageViewModel
        {
            public ISource<Unit> Taken => _taken;

            private readonly Signal<Unit> _taken = new();

            public void Take() =>
                _taken.Fire(Unit.Instance);
        }
    }
}
