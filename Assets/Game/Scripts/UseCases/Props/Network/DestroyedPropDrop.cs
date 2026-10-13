using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.Network
{
    public sealed class DestroyedPropDrop
    {
        private readonly IWeapon _prop;
        private readonly IWeaponInventories _inventories;

        public DestroyedPropDrop(
            Lifetime lifetime,
            IWeapon prop,
            IReadonlyMortal mortal,
            INetworkLifetimedObject networkObject,
            IWeaponInventories inventories)
        {
            _prop = prop;
            _inventories = inventories;

            mortal.State.WhenDead(lifetime, _ => Drop());
            networkObject.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime => spawnedLifetime.OnTermination(Drop));
        }

        private void Drop()
        {
            if (_inventories.TryFindInventoryCarrying(_prop, out var inventory))
                inventory.DropAndDrawNext(_prop);
        }
    }
}
