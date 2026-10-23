using Bw.Entities;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public interface IWeaponControlledBy
    {
        public IViewableMap<IWeapon, IControlledBy> ByWeapon { get; }
    }
}
