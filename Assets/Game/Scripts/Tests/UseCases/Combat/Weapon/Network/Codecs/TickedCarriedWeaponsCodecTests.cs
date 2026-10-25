using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.UseCases.Combat.Weapon.Network.Codecs;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Tests.UseCases.Combat.Weapon.Network.Codecs
{
    public sealed class TickedCarriedWeaponsCodecTests
    {
        [Test]
        public void CarriedWeaponsSurviveTheWire()
        {
            var weapons = new[] { new CarriedWeapon(7, 0), new CarriedWeapon(123456789, 1), new CarriedWeapon(ulong.MaxValue, 2) };

            var decoded = RoundTrip(new TickedState<CarriedWeapons>(5000, new CarriedWeapons(weapons)), out _);

            Assert.That(decoded.Tick, Is.EqualTo(5000));
            Assert.That(decoded.State.Weapons, Is.EqualTo(weapons));
        }

        [Test]
        public void EmptyHandsComeBackEmpty()
        {
            var decoded = RoundTrip(new TickedState<CarriedWeapons>(5, new CarriedWeapons(new CarriedWeapon[0])), out var length);

            Assert.That(decoded.State.Weapons, Is.Empty);
            Assert.That(length, Is.EqualTo(2));
        }

        [Test]
        public void SmallIdTakesTwoBytesWithItsSlot()
        {
            RoundTrip(new TickedState<CarriedWeapons>(5, new CarriedWeapons(new[] { new CarriedWeapon(7, 1) })), out var length);

            Assert.That(length, Is.EqualTo(4));
        }

        [Test]
        public void OrderOfTheWeaponsIsKept()
        {
            var weapons = new[] { new CarriedWeapon(3, 2), new CarriedWeapon(1, 0), new CarriedWeapon(2, 1) };

            var decoded = RoundTrip(new TickedState<CarriedWeapons>(1, new CarriedWeapons(weapons)), out _);

            Assert.That(decoded.State.Weapons, Is.EqualTo(weapons));
        }

        private static TickedState<CarriedWeapons> RoundTrip(TickedState<CarriedWeapons> sent, out int length)
        {
            using var writer = new FastBufferWriter(256, Allocator.Temp);
            writer.WriteNetworkSerializable(new TickedCarriedWeaponsCodec { Value = sent });
            length = writer.Length;
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out TickedCarriedWeaponsCodec decoded);
            return decoded.Value;
        }
    }
}
