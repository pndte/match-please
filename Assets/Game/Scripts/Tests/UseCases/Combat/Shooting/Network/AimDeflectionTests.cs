using Bw.Tests.Doubles;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Shooting.Network;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Shooting.Network
{
    public sealed class AimDeflectionTests
    {
        private ShootingWeaponConfig _config;
        private ManualClock _clock;
        private AimDeflection _deflection;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<ShootingWeaponConfig>();
            _config.Spread = new SpreadConfig { Climb = 5f, Jitter = 0f, Return = 8f };
            _clock = new ManualClock(0.125f);
            _deflection = new AimDeflection(new Spread(_config), _clock);
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [Test]
        public void BarrelStartsOnTheAim()
        {
            _clock.Tick(40);

            Assert.That(_deflection.Current, Is.EqualTo(0f));
        }

        [Test]
        public void KickThrowsTheBarrelAtOnce()
        {
            _clock.Tick(10);

            _deflection.Kick(5f);

            Assert.That(_deflection.Current, Is.EqualTo(5f));
        }

        [Test]
        public void KickedBarrelReturnsAtTheSpreadsRateOverTicks()
        {
            _clock.Tick(10);
            _deflection.Kick(5f);

            _clock.Tick(12);

            Assert.That(_deflection.Current, Is.EqualTo(3f).Within(1e-5f));
        }

        [Test]
        public void BarrelComesBackToTheAimAndStaysThere()
        {
            _clock.Tick(10);
            _deflection.Kick(5f);

            _clock.Tick(30);

            Assert.That(_deflection.Current, Is.EqualTo(0f));
        }

        [Test]
        public void NextKickAddsToWhatIsLeftOfTheLast()
        {
            _clock.Tick(0);
            _deflection.Kick(5f);
            _clock.Tick(2);

            _deflection.Kick(4f);
            _clock.Tick(3);

            Assert.That(_deflection.Current, Is.EqualTo(6f).Within(1e-5f));
        }

        [Test]
        public void DownwardKickReturnsUpToTheAim()
        {
            _clock.Tick(5);
            _deflection.Kick(-3f);

            _clock.Tick(7);

            Assert.That(_deflection.Current, Is.EqualTo(-1f).Within(1e-5f));
        }
    }
}
