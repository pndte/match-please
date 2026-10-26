using Bw.Entities.Infrastructure;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class WeaponCollections : IWeaponCollections
    {
        public IViewableBiMap<ulong, IWeapon> ByIds { get; }

        public WeaponCollections(Lifetime lifetime)
        {
            ByIds = new ViewableBiMap<ulong, IWeapon>(lifetime);
        }
    }
}
