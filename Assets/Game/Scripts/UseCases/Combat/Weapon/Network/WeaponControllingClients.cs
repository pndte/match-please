using Bw.Entities.Network;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class WeaponControllingClients : IWeaponControllingClients
    {
        public IViewableMap<IWeapon, IControllingClients> ByWeapon { get; } = new ViewableMap<IWeapon, IControllingClients>();
    }
}
