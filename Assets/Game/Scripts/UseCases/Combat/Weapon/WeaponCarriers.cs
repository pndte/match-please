using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon
{
    public sealed class WeaponCarriers : IWeaponCarriers
    {
        public IViewableMap<IWeapon, ICharacter> ByWeapon { get; } = new ViewableMap<IWeapon, ICharacter>();
    }
}
