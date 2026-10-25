using System.Collections.Generic;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;

namespace Bw.Tests.UseCases.PropsMatchBots.Doubles
{
    public sealed class WeaponDescriptions : IReadonlyWeaponDescriptionRegistry
    {
        private readonly Dictionary<IReadonlyWeapon, WeaponDescription> _issued = new();

        public void Issue(IReadonlyWeapon weapon, WeaponDescription description) =>
            _issued.Add(weapon, description);

        public WeaponDescription DescriptionOf(IReadonlyWeapon weapon) =>
            _issued[weapon];
    }
}
