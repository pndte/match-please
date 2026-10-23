using Bw.Entities.Infrastructure;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public interface INetworkWeapons
    {
        public IViewableBiMap<ulong, IWeapon> ByIds { get; }
    }
}
