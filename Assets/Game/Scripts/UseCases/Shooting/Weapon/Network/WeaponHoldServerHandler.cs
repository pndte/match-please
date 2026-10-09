using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Character;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class WeaponHoldServerHandler
    {
        private readonly ICharacterRegistry _characterRegistry;
        private readonly IGameObjectByCharacterCollection _characterObjects;
        private readonly IOwnershipController _ownershipController;
        private readonly IControlledBy _controlledBy;
        private readonly NetworkObject _networkObject;

        public WeaponHoldServerHandler(
            Lifetime lifetime,
            IWeapon weapon,
            IHeldWeaponCollection heldWeapons,
            ICharacterRegistry characterRegistry,
            IGameObjectByCharacterCollection characterObjects,
            IOwnershipController ownershipController,
            IControlledBy controlledBy,
            NetworkObject networkObject)
        {
            _characterRegistry = characterRegistry;
            _characterObjects = characterObjects;
            _ownershipController = ownershipController;
            _controlledBy = controlledBy;
            _networkObject = networkObject;

            heldWeapons.ByCharacter.Inverse.ViewForKey(lifetime, weapon, Hold);
        }

        private void Hold(Lifetime heldLifetime, IReadonlyCharacter character)
        {
            var player = _characterRegistry.PlayerByCharacter[character];
            _ownershipController.AddOwner(heldLifetime, player);
            _controlledBy.Set(heldLifetime, player);
            AttachTo(heldLifetime, _characterObjects[character]);
        }

        private void AttachTo(Lifetime heldLifetime, GameObject character)
        {
            if (!_networkObject.TrySetParent(character.GetComponent<NetworkObject>(), worldPositionStays: false))
                throw new InvalidOperationException(
                    $"Weapon {_networkObject.NetworkObjectId.ToString()} can't be attached to character '{character.name}'.");

            heldLifetime.OnTermination(Detach);
        }

        private void Detach()
        {
            if (_networkObject.IsSpawned && !_networkObject.TryRemoveParent())
                throw new InvalidOperationException(
                    $"Weapon {_networkObject.NetworkObjectId.ToString()} can't be detached from its holder.");
        }
    }
}
