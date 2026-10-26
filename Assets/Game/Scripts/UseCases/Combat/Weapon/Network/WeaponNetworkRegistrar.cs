using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class WeaponNetworkRegistrar
    {
        public WeaponNetworkRegistrar(
            Lifetime lifetime,
            INetworkLifetimedObject networkLifetimed,
            NetworkObject networkObject,
            IWeapon weapon,
            IWeaponCollections weapons)
        {
            networkLifetimed.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                weapons.ByIds.AddLifetimed(spawnedLifetime, networkObject.NetworkObjectId, weapon));
        }
    }
}
