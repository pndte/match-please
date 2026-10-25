using System;
using System.Collections.Generic;
using Bw.Entities;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.Entities
{
    public sealed class HealthTests
    {
        private LifetimeDefinition _lifetime;
        private HealthConfig _config;
        private Health _health;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _config = ScriptableObject.CreateInstance<HealthConfig>();
            _config.Max = 100f;
            _health = new Health(_config);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void NewHealthIsFullWithoutAChange()
        {
            Assert.That(_health.Max, Is.EqualTo(100f));
            Assert.That(_health.State.Value.Current, Is.EqualTo(100f));
            Assert.That(_health.State.Value.Change, Is.EqualTo(0f));
        }

        [Test]
        public void MaxFollowsTheConfig()
        {
            _config.Max = 150f;

            Assert.That(_health.Max, Is.EqualTo(150f));
        }

        [Test]
        public void AppliedStateBecomesTheState()
        {
            _health.Apply(new HealthState(60f, -40f));

            Assert.That(_health.State.Value, Is.EqualTo(new HealthState(60f, -40f)));
        }

        [Test]
        public void ChangeOfAnAppliedStateIsAnnounced()
        {
            var changes = new List<float>();
            _health.Changed.Advise(_lifetime.Lifetime, changes.Add);

            _health.Apply(new HealthState(60f, -40f));
            _health.Apply(new HealthState(70f, 10f));

            Assert.That(changes, Is.EqualTo(new[] { -40f, 10f }));
        }

        [Test]
        public void StateWithoutAChangeIsAppliedSilently()
        {
            var changes = new List<float>();
            _health.Changed.Advise(_lifetime.Lifetime, changes.Add);

            _health.Apply(new HealthState(60f, 0f));

            Assert.That(changes, Is.Empty);
            Assert.That(_health.State.Value.Current, Is.EqualTo(60f));
        }

        [TestCase(0f)]
        [TestCase(100f)]
        public void HealthAtTheBoundsIsAccepted(float current)
        {
            _health.Apply(new HealthState(current, 0f));

            Assert.That(_health.State.Value.Current, Is.EqualTo(current));
        }

        [TestCase(-0.01f)]
        [TestCase(100.01f)]
        public void HealthOutsideTheBoundsIsRefusedWithoutAnyEffect(float current)
        {
            var changes = new List<float>();
            _health.Changed.Advise(_lifetime.Lifetime, changes.Add);

            Assert.Throws<ArgumentOutOfRangeException>(() => _health.Apply(new HealthState(current, -1f)));
            Assert.That(changes, Is.Empty);
            Assert.That(_health.State.Value.Current, Is.EqualTo(100f));
        }
    }
}
