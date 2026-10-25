using Bw.Tests.Doubles;
using Bw.UseCases.Combat.Melee;
using Bw.UseCases.Combat.Melee.Extensions;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Combat.Melee.Extensions
{
    public sealed class MeleeWeaponConfigExtensionsTests
    {
        private MeleeWeaponConfig _config;
        private ManualClock _step;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<MeleeWeaponConfig>();
            _step = new ManualClock(0.125f);
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [TestCase(0.5f, 4)]
        [TestCase(0.55f, 5)]
        [TestCase(0.01f, 1)]
        [TestCase(0f, 1)]
        public void SwingLastsItsTimeRoundedUpToAtLeastOneTick(float time, int ticks)
        {
            _config.SwingTime = time;

            Assert.That(_config.SwingTicks(_step), Is.EqualTo(ticks));
        }

        [TestCase(0.25f, 2)]
        [TestCase(0.27f, 2)]
        [TestCase(0f, 0)]
        [TestCase(1f, 3)]
        public void StrikeComesAtItsNearestTickInsideTheSwing(float time, int ticks)
        {
            _config.SwingTime = 0.5f;
            _config.StrikeTime = time;

            Assert.That(_config.StrikeTicks(_step), Is.EqualTo(ticks));
        }

        [TestCase(0.75f, 6)]
        [TestCase(0.8f, 7)]
        [TestCase(0f, 1)]
        public void StrongSwingLastsItsTimeRoundedUpToAtLeastOneTick(float time, int ticks)
        {
            _config.StrongSwingTime = time;

            Assert.That(_config.StrongSwingTicks(_step), Is.EqualTo(ticks));
        }

        [TestCase(0.25f, 2)]
        [TestCase(2f, 5)]
        public void StrongStrikeComesAtItsNearestTickInsideTheStrongSwing(float time, int ticks)
        {
            _config.StrongSwingTime = 0.75f;
            _config.StrongStrikeTime = time;

            Assert.That(_config.StrongStrikeTicks(_step), Is.EqualTo(ticks));
        }

        [TestCase(0.5f, 4)]
        [TestCase(0.3f, 3)]
        [TestCase(0f, 0)]
        public void MinimumChargeIsItsTimeRoundedUp(float time, int ticks)
        {
            _config.MinChargeTime = time;

            Assert.That(_config.MinChargeTicks(_step), Is.EqualTo(ticks));
        }

        [TestCase(1f, 8)]
        [TestCase(0.01f, 1)]
        [TestCase(0f, 1)]
        public void FullChargeTakesItsTimeRoundedUpToAtLeastOneTick(float time, int ticks)
        {
            _config.FullChargeTime = time;

            Assert.That(_config.FullChargeTicks(_step), Is.EqualTo(ticks));
        }

        [TestCase(0.5f, 4)]
        [TestCase(0f, 0)]
        public void FullChargeHoldIsItsTimeRoundedUp(float time, int ticks)
        {
            _config.FullChargeHoldTime = time;

            Assert.That(_config.FullChargeHoldTicks(_step), Is.EqualTo(ticks));
        }

        [TestCase(0.25f, 2)]
        [TestCase(0.2f, 2)]
        [TestCase(0f, 0)]
        public void RecoveryIsItsTimeRoundedUp(float time, int ticks)
        {
            _config.RecoveryTime = time;

            Assert.That(_config.RecoveryTicks(_step), Is.EqualTo(ticks));
        }

        [TestCase(0, 0f)]
        [TestCase(2, 0.25f)]
        [TestCase(4, 0.5f)]
        [TestCase(8, 1f)]
        [TestCase(12, 1f)]
        public void ChargeIsTheShareOfTheFullChargeCappedAtOne(int chargeTicks, float charge)
        {
            _config.FullChargeTime = 1f;

            Assert.That(_config.Charge(_step, chargeTicks), Is.EqualTo(charge));
        }

        [Test]
        public void ShippedFistsSwingInNineTicksAndChargeInThirtyAtThirtyHertz()
        {
            var shipped = UnityEditor.AssetDatabase.LoadAssetAtPath<MeleeWeaponConfig>("Assets/Resources/Weapons/FistsMeleeConfig.asset");
            var step = new ManualClock(1f / 30f);

            Assert.That(shipped.SwingTicks(step), Is.EqualTo(9));
            Assert.That(shipped.StrikeTicks(step), Is.EqualTo(3));
            Assert.That(shipped.StrongSwingTicks(step), Is.EqualTo(14));
            Assert.That(shipped.StrongStrikeTicks(step), Is.EqualTo(3));
            Assert.That(shipped.MinChargeTicks(step), Is.EqualTo(9));
            Assert.That(shipped.FullChargeTicks(step), Is.EqualTo(30));
            Assert.That(shipped.FullChargeHoldTicks(step), Is.EqualTo(24));
            Assert.That(shipped.RecoveryTicks(step), Is.Zero);
        }
    }
}
