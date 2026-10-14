using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using Unity.Netcode;
using Unity.Netcode.Components;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class HeldWeaponTransformPause
    {
        private readonly INetworkLifetimedObject _networkLifetimed;
        private readonly NetworkObject _networkObject;
        private readonly NetworkTransform _networkTransform;

        public HeldWeaponTransformPause(
            Lifetime lifetime,
            IWeaponHold hold,
            INetworkLifetimedObject networkLifetimed,
            NetworkObject networkObject,
            NetworkTransform networkTransform)
        {
            _networkLifetimed = networkLifetimed;
            _networkObject = networkObject;
            _networkTransform = networkTransform;

            hold.HeldLifetime.WhenAlive(lifetime, Pause);
        }

        private void Pause(Lifetime heldLifetime)
        {
            var synced = SyncedAxesOf(_networkTransform);
            Sync(_networkTransform, SyncedAxes.None);
            heldLifetime.OnTermination(() => Resume(synced));
        }

        private void Resume(SyncedAxes synced)
        {
            Sync(_networkTransform, synced);
            if (!_networkLifetimed.SpawnedLifetime.Value.IsAlive)
                return;

            var released = _networkObject.transform;
            _networkTransform.Teleport(released.position, released.rotation, released.localScale);
        }

        private static SyncedAxes SyncedAxesOf(NetworkTransform networkTransform) =>
            new(
                (networkTransform.SyncPositionX, networkTransform.SyncPositionY, networkTransform.SyncPositionZ),
                (networkTransform.SyncRotAngleX, networkTransform.SyncRotAngleY, networkTransform.SyncRotAngleZ),
                (networkTransform.SyncScaleX, networkTransform.SyncScaleY, networkTransform.SyncScaleZ));

        private static void Sync(NetworkTransform networkTransform, SyncedAxes axes)
        {
            (networkTransform.SyncPositionX, networkTransform.SyncPositionY, networkTransform.SyncPositionZ) = axes.Position;
            (networkTransform.SyncRotAngleX, networkTransform.SyncRotAngleY, networkTransform.SyncRotAngleZ) = axes.Rotation;
            (networkTransform.SyncScaleX, networkTransform.SyncScaleY, networkTransform.SyncScaleZ) = axes.Scale;
        }

        private readonly struct SyncedAxes
        {
            public static readonly SyncedAxes None = default;

            public readonly (bool X, bool Y, bool Z) Position;
            public readonly (bool X, bool Y, bool Z) Rotation;
            public readonly (bool X, bool Y, bool Z) Scale;

            public SyncedAxes((bool X, bool Y, bool Z) position, (bool X, bool Y, bool Z) rotation, (bool X, bool Y, bool Z) scale)
            {
                Position = position;
                Rotation = rotation;
                Scale = scale;
            }
        }
    }
}
