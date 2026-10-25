using Bw.UseCases.Combat.Weapon.Network.Codecs;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Weapon.Network.Codecs
{
    public sealed class WeaponPickupCodecTests
    {
        private const float HalfStep = 1f / 32f;

        [TestCase(0f, 0f)]
        [TestCase(1.5f, -2.25f)]
        [TestCase(-37.1f, 12.03f)]
        [TestCase(250.97f, -88.4f)]
        public void PointSurvivesTheWireWithinHalfASixteenth(float x, float y)
        {
            var decoded = RoundTrip(new WeaponPickup(new Vector2(x, y)), out _);

            Assert.That(decoded.Point.x, Is.EqualTo(x).Within(HalfStep + 1e-5f));
            Assert.That(decoded.Point.y, Is.EqualTo(y).Within(HalfStep + 1e-5f));
        }

        [Test]
        public void PointOnTheGridComesBackExactly()
        {
            var decoded = RoundTrip(new WeaponPickup(new Vector2(1.5f, -2.25f)), out _);

            Assert.That(decoded.Point, Is.EqualTo(new Vector2(1.5f, -2.25f)));
        }

        [TestCase(0f, 0f, 2)]
        [TestCase(1.5f, -2.25f, 4)]
        public void PointTakesAPackedNumberPerAxis(float x, float y, int bytes)
        {
            RoundTrip(new WeaponPickup(new Vector2(x, y)), out var length);

            Assert.That(length, Is.EqualTo(bytes));
        }

        private static WeaponPickup RoundTrip(WeaponPickup sent, out int length)
        {
            using var writer = new FastBufferWriter(64, Allocator.Temp);
            writer.WriteNetworkSerializable(new WeaponPickupCodec { Value = sent });
            length = writer.Length;
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out WeaponPickupCodec decoded);
            return decoded.Value;
        }
    }
}
