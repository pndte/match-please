using Bw.Entities.Network.Objects;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public class NetworkWeaponHold : NetworkLifetimedBehaviour, IWeaponHold
    {
        public IReadonlyProperty<Lifetime> HeldLifetime => _heldLifetime;

        private readonly ViewableProperty<Lifetime> _heldLifetime = new(Lifetime.Terminated);
        private SequentialLifetimes _holds;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            var spawnedLifetime = SpawnedLifetime.Value;
            _holds = new SequentialLifetimes(spawnedLifetime);
            spawnedLifetime.OnTermination(() => _heldLifetime.Value = Lifetime.Terminated);
            FollowParent();
        }

        public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
        {
            if (IsSpawned)
                FollowParent();
        }

        private void FollowParent()
        {
            if (transform.parent == null)
            {
                _holds.TerminateCurrent();
                _heldLifetime.Value = Lifetime.Terminated;
                return;
            }

            _heldLifetime.Value = _holds.Next();
        }
    }
}
