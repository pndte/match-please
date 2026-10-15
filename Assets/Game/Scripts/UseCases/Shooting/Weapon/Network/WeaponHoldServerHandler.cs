using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Character;
using Bw.UseCases.Mortality;
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
        private readonly IOwnerRegistry _ownerRegistry;
        private readonly IControllerRegistry _controllerRegistry;
        private readonly IWeaponDraw _draw;
        private readonly NetworkObject _networkObject;

        public WeaponHoldServerHandler(
            Lifetime lifetime,
            IWeapon weapon,
            IWeaponInventories inventories,
            ICharacterRegistry characterRegistry,
            IGameObjectByCharacterCollection characterObjects,
            IOwnerRegistry ownerRegistry,
            IControllerRegistry controllerRegistry,
            IWeaponDraw draw,
            NetworkObject networkObject)
        {
            _characterRegistry = characterRegistry;
            _characterObjects = characterObjects;
            _ownerRegistry = ownerRegistry;
            _controllerRegistry = controllerRegistry;
            _draw = draw;
            _networkObject = networkObject;

            inventories.Carriers.ViewForKey(lifetime, weapon, Hold);
            inventories.Drawn.Inverse.ViewForKey(lifetime, weapon, Draw);
        }

        private void Hold(Lifetime heldLifetime, IReadonlyMortal character)
        {
            _ownerRegistry.AddOwner(heldLifetime, _characterRegistry.PlayerByCharacter[character]);
            AttachTo(heldLifetime, _characterObjects[character]);
        }

        private void Draw(Lifetime drawnLifetime, IReadonlyMortal character)
        {
            _controllerRegistry.Set(drawnLifetime, _characterRegistry.PlayerByCharacter[character]);
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
        }
    }
}
