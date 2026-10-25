using Bw.UseCases.Combat.Shooting;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Shooting
{
    public sealed class FixedShotPowerTests
    {
        private ShootingWeaponConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<ShootingWeaponConfig>();
            _config.Damage = 12f;
            _config.Knockback = 2f;
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [Test]
        public void HitCarriesTheConfiguredDamageAndKnockbackAlongTheShot()
        {
            var hit = new FixedShotPower(_config).Along(Vector2.up, default);

            Assert.That(hit.Damage, Is.EqualTo(12f));
            Assert.That(hit.Knockback, Is.EqualTo(new Vector2(0f, 2f)));
        }

        [Test]
        public void HitIsTheSameWhateverTheGunState()
        {
            var power = new FixedShotPower(_config);

            var fresh = power.Along(Vector2.left, new GunState(0f, 6, 0, 0, 0));
            var worn = power.Along(Vector2.left, new GunState(90f, 1, 3, 2, 40));

            Assert.That(worn.Damage, Is.EqualTo(fresh.Damage));
            Assert.That(worn.Knockback, Is.EqualTo(fresh.Knockback));
        }

        [Test]
        public void HitReadsTheConfigWhenItIsMade()
        {
            var power = new FixedShotPower(_config);
            _config.Damage = 30f;

            Assert.That(power.Along(Vector2.right, default).Damage, Is.EqualTo(30f));
        }
    }
}
