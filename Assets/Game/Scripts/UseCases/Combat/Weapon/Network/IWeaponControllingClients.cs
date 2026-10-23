using Bw.Entities.Network;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public interface IWeaponControllingClients
    {
        public IViewableMap<IWeapon, IControllingClients> ByWeapon { get; }
    }
}
