using Bw.Entities.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.Entities.Extensions
{
    public sealed class Vector2ExtensionsTests
    {
        [TestCase(1f, 2f, 5f)]
        [TestCase(-3f, 0f, 0f)]
        [TestCase(0f, 7f, -1.5f)]
        public void WithXReplacesOnlyX(float x, float y, float newX)
        {
            Assert.That(new Vector2(x, y).WithX(newX), Is.EqualTo(new Vector2(newX, y)));
        }

        [TestCase(1f, 2f, 5f)]
        [TestCase(-3f, 0f, 0f)]
        [TestCase(0f, 7f, -1.5f)]
        public void WithYReplacesOnlyY(float x, float y, float newY)
        {
            Assert.That(new Vector2(x, y).WithY(newY), Is.EqualTo(new Vector2(x, newY)));
        }

        [TestCase(1f, 2f, 3f, 10f)]
        [TestCase(-1f, 0f, 4f, 0f)]
        public void WithZReplacesOnlyZ(
            float x,
            float y,
            float z,
            float newZ)
        {
            Assert.That(new Vector3(x, y, z).WithZ(newZ), Is.EqualTo(new Vector3(x, y, newZ)));
        }
    }
}
