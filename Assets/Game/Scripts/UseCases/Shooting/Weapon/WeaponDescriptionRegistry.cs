using System;
using System.Collections.Generic;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class WeaponDescriptionRegistry : IWeaponDescriptionRegistry
    {
        private readonly Dictionary<IReadonlyWeapon, WeaponDescription> _descriptions = new();

        public WeaponDescription DescriptionOf(IReadonlyWeapon weapon)
        {
            if (!_descriptions.TryGetValue(weapon, out var description))
                throw new InvalidOperationException("The weapon has no description: every gun and the fists register theirs while they are spawned.");

            return description;
        }

        public void AddLifetimed(Lifetime lifetime, IReadonlyWeapon weapon, WeaponDescription description)
        {
            if (!_descriptions.TryAdd(weapon, description))
                throw new InvalidOperationException("A weapon has one description at a time: it was registered again before its last registration ended.");

            lifetime.OnTermination(() => _descriptions.Remove(weapon));
        }
    }
}
