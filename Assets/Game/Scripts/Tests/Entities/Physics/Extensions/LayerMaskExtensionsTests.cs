using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.Entities.Physics.Extensions
{
    public sealed class LayerMaskExtensionsTests
    {
        [TestCase(0, 0ul)]
        [TestCase(1 << 14, 1ul << 14)]
        [TestCase((1 << 14) | (1 << 15) | (1 << 16), 114688ul)]
        [TestCase(1 << 31, 1ul << 31)]
        public void LayerBitsBecomeTheSameCategoryBits(int layers, ulong categories)
        {
            LayerMask mask = layers;

            Assert.That(mask.AsPhysicsMask().bitMask, Is.EqualTo(categories));
        }

        [Test]
        public void EverythingCoversTheLayersButNotThePhysicsOnlyCategories()
        {
            LayerMask everything = ~0;

            var categories = everything.AsPhysicsMask();

            Assert.That(categories.bitMask, Is.EqualTo(0xFFFFFFFFul));
            Assert.That(categories.IsBitSet(PhysicsCategories.OneWay), Is.False);
        }
    }
}
