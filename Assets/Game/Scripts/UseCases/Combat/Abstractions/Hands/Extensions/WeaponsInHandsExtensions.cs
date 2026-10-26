using System;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Combat.Abstractions.Hands.Extensions
{
    public static class WeaponsInHandsExtensions
    {
        public static IReadonlyCharacter HolderOf(this IReadonlyWeaponsInHands inHands, IWeapon weapon) =>
            inHands.ByWeapon.TryGetValue(weapon, out var holder)
                ? holder
                : throw new InvalidOperationException("The weapon is in nobody's hands: only a weapon in hands aims, strikes or roots its holder.");
    }
}
