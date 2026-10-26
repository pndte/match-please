using System;
using System.Collections.Generic;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class KindRegisteringPrefabHandler : INetworkPrefabInstanceHandler
    {
        private readonly Dictionary<NetworkObject, LifetimeDefinition> _registered = new();
        private readonly Lifetime _lifetime;
        private readonly INetworkPrefabInstanceHandler _handler;
        private readonly WeaponKind _kind;
        private readonly IWeaponKindRegistry _kinds;

        public KindRegisteringPrefabHandler(
            Lifetime lifetime,
            INetworkPrefabInstanceHandler handler,
            WeaponKind kind,
            IWeaponKindRegistry kinds)
        {
            _lifetime = lifetime;
            _handler = handler;
            _kind = kind;
            _kinds = kinds;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            var instance = _handler.Instantiate(ownerClientId, position, rotation);
            var registered = _lifetime.CreateNested();
            _kinds.AddLifetimed(registered.Lifetime, WeaponOf(instance), _kind);
            _registered.Add(instance, registered);
            return instance;
        }

        public void Destroy(NetworkObject networkObject)
        {
            _registered[networkObject].Terminate();
            _registered.Remove(networkObject);
            _handler.Destroy(networkObject);
        }

        private static IWeapon WeaponOf(NetworkObject instance) =>
            instance.TryGetComponent(out WeaponHolder holder)
                ? holder.Value
                : throw new InvalidOperationException($"'{instance.name}' has no WeaponHolder after its prefab install: only a weapon or a prop has a kind.");
    }
}
