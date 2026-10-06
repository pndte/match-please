using Bw.Entities.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using Unity.Netcode.Components;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class HeldWeaponNetworkTransform //TODO: сервер продолжает слать через NetworkTransform позу оружия в руках, хотя клиенты её не применяют — выключать отправку на время удержания
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
