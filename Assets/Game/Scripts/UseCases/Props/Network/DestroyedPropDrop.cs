using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.Network
{
    public sealed class DestroyedPropDrop
    {
        private readonly IWeapon _prop;
        private readonly IWeaponCarriers _carriers;

        public DestroyedPropDrop(
            Lifetime lifetime,
            IWeapon prop,
            IReadonlyMortal mortal,
            INetworkLifetimedObject networkObject,
            IWeaponCarriers carriers)
        {
            _prop = prop;
            _carriers = carriers;

            mortal.State.WhenDead(lifetime, _ => Drop());
            networkObject.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime => spawnedLifetime.OnTermination(Drop));
        }

        private void Drop()
        {
            if (_carriers.ByWeapon.TryGetValue(_prop, out var carrier))
                carrier.Inventory.Drop(_prop);
        }
    }
}
