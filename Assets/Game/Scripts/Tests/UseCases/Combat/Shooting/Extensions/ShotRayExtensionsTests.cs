using Bw.UseCases.Combat.Shooting.Extensions;
using Bw.UseCases.Combat.Weapon;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Shooting.Extensions
{
    public sealed class ShotRayExtensionsTests
    {
        private static readonly ShotRay Ray = new(new Vector2(1f, 2f), new Vector2(3f, 2f), Vector2.right);

        [TestCase(0f, 1f, 0f)]
        [TestCase(90f, 0f, 1f)]
        [TestCase(-90f, 0f, -1f)]
        [TestCase(180f, -1f, 0f)]
        [TestCase(45f, 0.70710678f, 0.70710678f)]
        public void TurnedRayPointsTheTurnedWay(float degrees, float x, float y)
        {
            var turned = Ray.Turned(degrees);

            Assert.That(turned.Direction.x, Is.EqualTo(x).Within(1e-5f));
            Assert.That(turned.Direction.y, Is.EqualTo(y).Within(1e-5f));
        }

        [Test]
        public void TurnedRayKeepsItsPivotAndMuzzle()
        {
            var turned = Ray.Turned(30f);

            Assert.That(turned.Pivot, Is.EqualTo(Ray.Pivot));
            Assert.That(turned.Muzzle, Is.EqualTo(Ray.Muzzle));
        }
    }
}
