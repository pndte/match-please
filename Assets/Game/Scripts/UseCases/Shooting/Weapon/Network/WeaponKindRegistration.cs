using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class WeaponKindRegistration
    {
        public WeaponKindRegistration(
            Lifetime lifetime,
            INetworkLifetimedObject networkObject,
            IWeapon weapon,
            IWeaponKindRegistry registry,
            WeaponKind kind)
        {
            networkObject.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                registry.AddLifetimed(spawnedLifetime, weapon, kind));
        }
    }
}
