using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Inventory.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Pickup.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class FloorWeaponRegistrar
    {
        public FloorWeaponRegistrar(
            Lifetime lifetime,
            INetworkLifetimedObject networkObject,
            IWeapon weapon,
            IWeaponCarriers carriers,
            IFloorWeapon floorWeapon,
            IFloorWeapons floorWeapons)
        {
            networkObject.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                carriers.ViewUncarried(spawnedLifetime, weapon, uncarriedLifetime => floorWeapons.AddLifetimed(uncarriedLifetime, floorWeapon)));
        }
    }
}
