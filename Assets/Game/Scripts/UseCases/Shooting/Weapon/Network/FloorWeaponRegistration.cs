using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Pickup.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class FloorWeaponRegistration
    {
        public FloorWeaponRegistration(
            Lifetime lifetime,
            INetworkLifetimedObject networkObject,
            IWeaponHold hold,
            IFloorWeapon weapon,
            IFloorWeapons weapons)
        {
            networkObject.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                hold.HeldLifetime.View(spawnedLifetime, (holdStateLifetime, heldLifetime) =>
                {
                    if (!heldLifetime.IsAlive)
                        weapons.AddLifetimed(holdStateLifetime, weapon);
                }));
        }
    }
}
