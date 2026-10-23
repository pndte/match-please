using Bw.Entities.Infrastructure;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class NetworkWeapons : INetworkWeapons
    {
        public IViewableBiMap<ulong, IWeapon> ByIds { get; }

        public NetworkWeapons(Lifetime lifetime)
        {
            ByIds = new ViewableBiMap<ulong, IWeapon>(lifetime);
        }
    }
}
