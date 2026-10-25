using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Extensions;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.Entities.Extensions
{
    public sealed class HealthExtensionsTests
    {
        private LifetimeDefinition _lifetime;
        private HealthConfig _config;
        private Health _health;
        private List<float> _damage;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _config = ScriptableObject.CreateInstance<HealthConfig>();
            _config.Max = 100f;
            _health = new Health(_config);
            _damage = new List<float>();
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void DamageIsReportedAsAPositiveAmount()
        {
            _health.AdviseDamage(_lifetime.Lifetime, _damage.Add);

            _health.Apply(new HealthState(70f, -30f));

            Assert.That(_damage, Is.EqualTo(new[] { 30f }));
        }

        [Test]
        public void HealIsNotDamage()
        {
            _health.Apply(new HealthState(50f, -50f));
            _health.AdviseDamage(_lifetime.Lifetime, _damage.Add);

            _health.Apply(new HealthState(80f, 30f));
            _health.Apply(new HealthState(80f, 0f));

            Assert.That(_damage, Is.Empty);
        }

        [Test]
        public void DamageBeforeTheSubscriptionIsNotReported()
        {
            _health.Apply(new HealthState(50f, -50f));

            _health.AdviseDamage(_lifetime.Lifetime, _damage.Add);

            Assert.That(_damage, Is.Empty);
        }

        [Test]
        public void DamageReportStopsWithItsLifetime()
        {
            var lifetime = _lifetime.Lifetime.CreateNested();
            _health.AdviseDamage(lifetime.Lifetime, _damage.Add);
            lifetime.Terminate();

            _health.Apply(new HealthState(70f, -30f));

            Assert.That(_damage, Is.Empty);
        }
    }
}
