using System.Linq;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.UseCases.Players;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Shooting.Weapon.Triggers.Network.Crutches
{
    public class WeaponShotTriggerSynchronizer : NetworkLifetimedBehaviour //TODO: лютый костыль, потом переделать под CustomMessages
    {
        private IWeapon _weapon;
        private IReadonlyControlledBy _controlledBy;
        private IClientPlayerCollection _clientPlayerCollection;

        [Inject]
        private void Construct(
            Lifetime lifetime,
            IWeapon weapon,
            IReadonlyControlledBy controlledBy,
            IClientPlayerCollection clientPlayerCollection)
        {
            _weapon = weapon;
            _controlledBy = controlledBy;
            _clientPlayerCollection = clientPlayerCollection;
            SpawnedLifetime.WhenAlive(lifetime, HandleSpawn); // TODO: bad practice
        }

        private void HandleSpawn(Lifetime lifetime)
        {
            if (!IsServer) return; // TODO: такого не должно быть здесь
            _weapon.Shot.Advise(lifetime, HandleShot);
        }

        private void HandleShot(Vector3 mousePosition)
        {
            var users = _controlledBy.Users.Select(player => _clientPlayerCollection.ByClient.Inverse[player].Id).ToArray();
            var rpcTarget = RpcTarget.Not(users, RpcTargetUse.Temp);
            
            InvokeShotRpc(mousePosition, new RpcParams { Send = new RpcSendParams {Target = rpcTarget}});
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void InvokeShotRpc(Vector3 mousePosition, RpcParams rpcParams = default)
        {
            if (IsServer) return; // TODO: КАЛ
            _weapon.ForceShot(mousePosition);
        }
    }
}