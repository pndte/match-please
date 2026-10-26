using Bw.UseCases.Combat.Weapon.Network.Codecs;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Tests.UseCases.Combat.Weapon.Network.Codecs
{
    public sealed class WeaponPickupCodecTests
    {
        [TestCase(0ul)]
        [TestCase(7ul)]
        [TestCase(123456789ul)]
        [TestCase(ulong.MaxValue)]
        public void WeaponIdSurvivesTheWire(ulong weapon)
        {
            var decoded = RoundTrip(new WeaponPickup(weapon), out _);

            Assert.That(decoded.Weapon, Is.EqualTo(weapon));
        }

        [Test]
        public void SmallIdTakesOneByte()
        {
            RoundTrip(new WeaponPickup(7), out var length);

            Assert.That(length, Is.EqualTo(1));
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
