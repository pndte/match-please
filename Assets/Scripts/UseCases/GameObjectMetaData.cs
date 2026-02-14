using System;
using System.Linq;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.UseCases.Players;
using Cysharp.Threading.Tasks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Setup;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases
{
    public class GameObjectMetaData : NetworkLifetimedBehaviour, IOwnership, IControlledBy //TODO: decompose to Server/Client scripts?
    {
        [Inject] private IRuntimeSettings _runtimeSettings;
        public IReadonlyProperty<bool> Me => _controllerByMe;
        public IReadonlyViewableList<IPlayer> Users => _users;
        public IReadonlyProperty<bool> Mine => _mine;
        public IReadonlyViewableList<IPlayer> Owners => _owners;
        
        private readonly ViewableProperty<bool> _controllerByMe = new(false);
        private readonly ViewableProperty<bool> _mine = new(false);
        private readonly BwViewableList<IPlayer> _owners = new();
        private readonly BwViewableList<IPlayer> _users = new();

        [Inject]
        private void Construct(
            Lifetime lifetime, 
            IClientPlayerCollection clientPlayerCollection,
            IPlayerCollection playerCollection)
        {
            if (_runtimeSettings.CurrentPeerType != PeerType.Server) return; //TODO: такого не должно быть в скриптах, это определяется в инсталлере
            
            SpawnedLifetime.WhenAlive(lifetime, aliveLifetime =>
            {
                var playerClientCollection = clientPlayerCollection.ByClient.Inverse;
                _owners.View(aliveLifetime, (ownerLifetime, player) =>
                {
                    if (!playerClientCollection.TryGetValue(player, out var client)) return;

                    var target = RpcTarget.Single(client.Id, RpcTargetUse.Persistent);
                    SetOwnershipForClientRpc(new RpcParams {Send = new RpcSendParams {Target = target}});

                    ownerLifetime.OnTermination(() =>
                    {
                        target.Dispose();
                        TerminateOwnershipForClientRpc(new RpcParams { Send = new RpcSendParams { Target = target } });
                    });
                });
            
                _users.View(aliveLifetime, (enabledLifetime, player) =>
                {
                    if (!playerClientCollection.TryGetValue(player, out var client)) return;

                    var target = RpcTarget.Single(client.Id, RpcTargetUse.Persistent);
                    SetEnabledForClientRpc(new RpcParams {Send = new RpcSendParams {Target = target}});

                    enabledLifetime.OnTermination(() =>
                    {
                        TerminateEnabledForClientRpc(new RpcParams { Send = new RpcSendParams { Target = target } });
                        target.Dispose();
                    });
                });
            });
        }

        public void AddOwner(Lifetime lifetime, IPlayer player)
        {
            _owners.AddLifetimed(lifetime, player);
        }

        public void Set(Lifetime lifetime, IPlayer player)
        {
            _users.AddLifetimed(lifetime, player);
        }
        
        [Rpc(SendTo.SpecifiedInParams)] //TODO: отказаться от рпс в пользу CustomMessages
        private void SetOwnershipForClientRpc(RpcParams rpcParams = default)
        {
            _mine.Value = true;
        }        
        
        [Rpc(SendTo.SpecifiedInParams)] //TODO: отказаться от рпс в пользу CustomMessages
        private void SetEnabledForClientRpc(RpcParams rpcParams = default)
        {
            _controllerByMe.Value = true;
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void TerminateOwnershipForClientRpc(RpcParams rpcParams = default)
        {
            _mine.Value = false; // TODO: на стороне сервера всегда надо проверять, корректно ли владение, и если нет, то не слушать клиента.
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void TerminateEnabledForClientRpc(RpcParams rpcParams = default)
        {
            _controllerByMe.Value = false; // TODO: на стороне сервера всегда надо проверять, корректно ли enabled, и если нет, то не слушать клиента.
        }
    }
}