using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class HeldWeaponBody
    {
        private readonly IWeapon _weapon;
        private readonly IWeaponCarriers _carriers;
        private readonly PhysicsBody _body;

        public HeldWeaponBody(
            Lifetime lifetime,
            INetworkLifetimedObject networkObject,
            IWeapon weapon,
            IWeaponCarriers carriers,
            PhysicsBody body)
        {
            _weapon = weapon;
            _carriers = carriers;
            _body = body;

            networkObject.SpawnedLifetime.WhenAlive(lifetime, Lie);
        }

        private void Lie(Lifetime spawnedLifetime)
        {
            Enter(spawnedLifetime);
            spawnedLifetime.OnTermination(Leave);
            _carriers.ByWeapon.ViewForKey(spawnedLifetime, _weapon, (carriedLifetime, _) =>
            {
                Leave();
                carriedLifetime.OnTermination(() => Enter(spawnedLifetime));
            });
        }

        private void Enter(Lifetime spawnedLifetime)
        {
            if (spawnedLifetime.IsAlive && _body.isValid)
                _body.enabled = true;
        }

        private void Leave()
        {
            if (_body.isValid)
                _body.enabled = false;
        }
    }
}
