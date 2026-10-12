using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Character;
using Bw.UseCases.Mortality;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class WeaponHoldServerHandler
    {
        private readonly ICharacterRegistry _characterRegistry;
        private readonly IGameObjectByCharacterCollection _characterObjects;
        private readonly IOwnershipController _ownershipController;
        private readonly IControlledBy _controlledBy;
        private readonly IWeaponDraw _draw;
        private readonly NetworkObject _networkObject;
        private readonly NetworkTransform _networkTransform;

        public WeaponHoldServerHandler(
            Lifetime lifetime,
            IWeapon weapon,
            IWeaponInventories inventories,
            ICharacterRegistry characterRegistry,
            IGameObjectByCharacterCollection characterObjects,
            IOwnershipController ownershipController,
            IControlledBy controlledBy,
            IWeaponDraw draw,
            NetworkObject networkObject,
            NetworkTransform networkTransform)
        {
            _characterRegistry = characterRegistry;
            _characterObjects = characterObjects;
            _ownershipController = ownershipController;
            _controlledBy = controlledBy;
            _draw = draw;
            _networkObject = networkObject;
            _networkTransform = networkTransform;

            inventories.Carriers.ViewForKey(lifetime, weapon, Hold);
            inventories.Drawn.Inverse.ViewForKey(lifetime, weapon, Draw);
        }

        private void Hold(Lifetime heldLifetime, IReadonlyMortal character)
        {
            _ownershipController.AddOwner(heldLifetime, _characterRegistry.PlayerByCharacter[character]);
            AttachTo(heldLifetime, _characterObjects[character]);
        }

        private void Draw(Lifetime drawnLifetime, IReadonlyMortal character)
        {
            _controlledBy.Set(drawnLifetime, _characterRegistry.PlayerByCharacter[character]);
            _draw.Draw(drawnLifetime);
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
            if (!_networkObject.IsSpawned)
                return;

            if (!_networkObject.TryRemoveParent())
                throw new InvalidOperationException(
                    $"Weapon {_networkObject.NetworkObjectId.ToString()} can't be detached from its holder.");

            RestartClientInterpolation();
        }

        private void RestartClientInterpolation()
        {
            var released = _networkObject.transform;
            _networkTransform.Teleport(released.position, released.rotation, released.localScale);
        }
    }
}
