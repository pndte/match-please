using Bw.UseCases.Combat.Shooting;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Shooting
{
    public sealed class SpreadTests
    {
        private ShootingWeaponConfig _config;
        private Spread _spread;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<ShootingWeaponConfig>();
            _config.Spread = new SpreadConfig { Climb = 5f, Jitter = 2.5f, Return = 10f };
            _spread = new Spread(_config);
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [TestCase(0f, 0f, 5f)]
        [TestCase(0f, 1f, 7.5f)]
        [TestCase(0f, -1f, 2.5f)]
        [TestCase(30f, 0.5f, 6.25f)]
        [TestCase(-60f, 0f, 5f)]
        [TestCase(90f, 0f, 5f)]
        [TestCase(-90f, 0f, 5f)]
        [TestCase(91f, 0f, -5f)]
        [TestCase(180f, 0f, -5f)]
        [TestCase(-135f, 1f, -2.5f)]
        [TestCase(450f, 0f, 5f)]
        [TestCase(-200f, -1f, -7.5f)]
        public void ThrowClimbsUpTheScreenForTheBarrelsFacingPlusTheRolledJitter(float aim, float roll, float thrown)
        {
            Assert.That(_spread.ThrowOf(aim, roll), Is.EqualTo(thrown).Within(1e-5f));
        }

        [TestCase(6f, 0.25f, 3.5f)]
        [TestCase(-6f, 0.25f, -3.5f)]
        [TestCase(1f, 1f, 0f)]
        [TestCase(-1f, 1f, 0f)]
        [TestCase(3f, 0f, 3f)]
        [TestCase(0f, 2f, 0f)]
        public void DeflectionReturnsToTheAimAtTheReturnRateWithoutOvershooting(float deflection, float seconds, float returned)
        {
            Assert.That(_spread.Returned(deflection, seconds), Is.EqualTo(returned).Within(1e-5f));
        }

        [Test]
        public void SpreadReadsTheConfigOnEveryCall()
        {
            _config.Spread = new SpreadConfig { Climb = 1f, Jitter = 0f, Return = 1f };

            Assert.That(_spread.ThrowOf(0f, 1f), Is.EqualTo(1f));
            Assert.That(_spread.Returned(2f, 1f), Is.EqualTo(1f));
        }
    }
}
