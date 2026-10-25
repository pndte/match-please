using Bw.UseCases.Combat.Weapon.Pickup;
using Bw.UseCases.Combat.Weapon.Pickup.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Weapon.Pickup.Extensions
{
    public sealed class WeaponPickupConfigExtensionsTests
    {
        private WeaponPickupConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<WeaponPickupConfig>();
            _config.Reach = 2.5f;
            _config.HoverRadius = 0.35f;
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [TestCase(2f, 0f, 0.1f, 0f, true)]
        [TestCase(2f, 0f, 0.35f, 0f, true)]
        [TestCase(2f, 0f, 0.36f, 0f, false)]
        [TestCase(2.5f, 0f, 0f, 0f, true)]
        [TestCase(2.6f, 0f, 0f, 0f, false)]
        [TestCase(0f, -2.6f, 0f, 0f, false)]
        [TestCase(2f, 2f, 0f, 0f, false)]
        [TestCase(2.6f, 0f, 0f, 1f, true)]
        [TestCase(3.5f, 0f, 0f, 1f, true)]
        [TestCase(3.6f, 0f, 0f, 1f, false)]
        public void WeaponIsPickableUnderTheCursorWithinReach(
            float x,
            float y,
            float pointDistance,
            float slack,
            bool pickable)
        {
            var holder = new Vector2(10f, 5f);

            Assert.That(_config.IsPickable(holder, holder + new Vector2(x, y), pointDistance, slack), Is.EqualTo(pickable));
        }
    }
}
