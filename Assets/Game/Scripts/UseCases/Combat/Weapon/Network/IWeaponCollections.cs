using Bw.Entities.Infrastructure;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public interface IWeaponCollections
    {
        public IViewableBiMap<ulong, IWeapon> ByIds { get; }
    }
}
