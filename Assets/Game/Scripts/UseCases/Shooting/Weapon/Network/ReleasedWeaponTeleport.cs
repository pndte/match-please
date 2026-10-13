using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using Unity.Netcode;
using Unity.Netcode.Components;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class ReleasedWeaponTeleport
    {
        private readonly INetworkLifetimedObject _networkLifetimed;
        private readonly NetworkObject _networkObject;
        private readonly NetworkTransform _networkTransform;

        public ReleasedWeaponTeleport(
            Lifetime lifetime,
            IWeaponHold hold,
            INetworkLifetimedObject networkLifetimed,
            NetworkObject networkObject,
            NetworkTransform networkTransform)
        {
            _networkLifetimed = networkLifetimed;
            _networkObject = networkObject;
            _networkTransform = networkTransform;

            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
                heldLifetime.OnTermination(RestartClientInterpolation));
        }

        private void RestartClientInterpolation()
        {
            if (!_networkLifetimed.SpawnedLifetime.Value.IsAlive)
                return;

            var released = _networkObject.transform;
            _networkTransform.Teleport(released.position, released.rotation, released.localScale);
        }
    }
}
