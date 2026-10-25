using System;
using Bw.Tests.UseCases.Combat.Hands.Doubles;
using Bw.UseCases.Combat.Abstractions.Hands.Extensions;
using Bw.UseCases.Combat.Characters;
using Bw.UseCases.Combat.Hands;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Abstractions.Hands
{
    public sealed class WeaponsInHandsExtensionsTests
    {
        [Test]
        public void HolderOfAWeaponInHandsIsTheCharacterHoldingIt()
        {
            var inHands = new WeaponsInHands();
            var weapon = new FakeWeapon();
            var character = new Bw.UseCases.Combat.Characters.Character(new FakeMortal(), null, null, null);
            inHands.ByWeapon.Add(weapon, character);

            Assert.That(inHands.HolderOf(weapon), Is.SameAs(character));
        }

        [Test]
        public void HolderOfAWeaponInNobodysHandsThrows()
        {
            var inHands = new WeaponsInHands();
            inHands.ByWeapon.Add(new FakeWeapon(), new Bw.UseCases.Combat.Characters.Character(new FakeMortal(), null, null, null));

            Assert.Throws<InvalidOperationException>(() => inHands.HolderOf(new FakeWeapon()));
        }
    }
}
