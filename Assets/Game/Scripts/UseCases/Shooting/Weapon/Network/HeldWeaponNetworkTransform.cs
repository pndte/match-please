using Bw.Entities.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using Unity.Netcode.Components;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class HeldWeaponNetworkTransform
    {
        public HeldWeaponNetworkTransform(Lifetime lifetime, IWeaponHold hold, NetworkTransform networkTransform)
        {
            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
            {
                networkTransform.enabled = false;
                heldLifetime.OnTermination(() => networkTransform.enabled = true);
            });
        }
    }
}
