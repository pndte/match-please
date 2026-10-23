using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class WeaponKindRegistration //TODO: предмет сам регистрирует свой вид, поэтому оружие, кулаки и ящик знают о IWeaponKindRegistry; регистрировать должен тот, кто их создаёт: Armory, спавнер ящиков или фабрика
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
