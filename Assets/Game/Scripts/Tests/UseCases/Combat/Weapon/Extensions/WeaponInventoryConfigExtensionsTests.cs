using System;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Extensions;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Combat.Weapon.Extensions
{
    public sealed class WeaponInventoryConfigExtensionsTests
    {
        private WeaponInventoryConfig _config;

        [SetUp]
        public void SetUp() =>
            _config = ScriptableObject.CreateInstance<WeaponInventoryConfig>();

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [TestCase(WeaponKind.Fists, false, false, false)]
        [TestCase(WeaponKind.Gun, true, false, false)]
        [TestCase(WeaponKind.Prop, true, true, true)]
        public void DefaultLayoutHasARowOfRulesForEveryKind(
            WeaponKind kind,
            bool droppable,
            bool handsOnly,
            bool pressOnly)
        {
            var rules = _config.RulesOf(kind);

            Assert.That(rules.Kind, Is.EqualTo(kind));
            Assert.That(rules.Droppable, Is.EqualTo(droppable));
            Assert.That(rules.HandsOnly, Is.EqualTo(handsOnly));
            Assert.That(rules.PressOnly, Is.EqualTo(pressOnly));
        }

        [Test]
        public void KindWithoutARowOfRulesThrows()
        {
            _config.Rules = new[] { new WeaponKindRules { Kind = WeaponKind.Gun, Droppable = true } };

            Assert.Throws<InvalidOperationException>(() => _config.RulesOf(WeaponKind.Prop));
        }

        [Test]
        public void FirstRowOfAKindWins()
        {
            _config.Rules = new[]
            {
                new WeaponKindRules { Kind = WeaponKind.Gun, Droppable = true },
                new WeaponKindRules { Kind = WeaponKind.Gun, Droppable = false }
            };

            Assert.That(_config.RulesOf(WeaponKind.Gun).Droppable, Is.True);
        }
    }
}
