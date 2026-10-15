using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class WeaponDescriptionRegistration //TODO: странно очень, что пушки себя регают, надо думать
                                                      //об отдельном сервисе для такого
    {
        public WeaponDescriptionRegistration(
            Lifetime lifetime,
            INetworkLifetimedObject networkObject,
            IReadonlyWeapon weapon,
            IWeaponDescriptionRegistry registry,
            WeaponDescription description)
        {
            networkObject.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                registry.AddLifetimed(spawnedLifetime, weapon, description));
        }
    }
}
