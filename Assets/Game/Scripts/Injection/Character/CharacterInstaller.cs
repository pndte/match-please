using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using Bw.Injection.ControlledBy;
using Bw.Injection.Network.Variables;
using JetBrains.Collections.Viewable;
using Bw.Injection.Ownership;
using Bw.UseCases;
using Bw.UseCases.Character;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Network;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public class CharacterInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [SerializeField] private HealthConfig _healthConfig;

        [SerializeField] private NetworkObject _networkObject;
        [SerializeField] private Rigidbody2D _physics;
        [SerializeField] private MovementConfig _movementConfig;
        [SerializeField] private NetworkMovementData _movementData;

        public override void InstallBindings()
        {
            Debug.Log("Character installer executed");

            OwnershipInstaller.Install(Container, _runtimeSettings);
            ControlledByInstaller.Install(Container, _runtimeSettings);
            
            Container.Bind<NetworkObject>().FromInstance(_networkObject).AsSingle();
            Container.Bind<INetworkLifetimedObject>().FromInstance(_movementData).AsSingle();

            NetTablesInstaller.Install(Container);
            var netFactory = Container.Resolve<INetPropertyFactory>();
            var netTable = Container.Resolve<INetVariablesTable>();//TODO: костыль, фиксить
            
            Container.Bind<HealthConfig>().FromInstance(_healthConfig).AsSingle();
            Container.Bind<Rigidbody2D>().FromInstance(_physics).AsSingle();
            Container.Bind<MovementConfig>().FromInstance(_movementConfig).AsSingle();
            var gameObjectLifetime = gameObject.Lifetime();
            Container.BindInstance(gameObjectLifetime).AsSingle();

            OwnershipServicesInstaller.Install(Container, _runtimeSettings, netFactory);
            ControlledByServicesInstaller.Install(Container, _runtimeSettings, netFactory);
            BindHealth(netFactory);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindInterfacesTo<ServerCharacter>().AsSingle();
                    Container.BindInterfacesAndSelfTo<Health>().AsSingle();
                    Container.Bind<DamageProcessor>().AsSingle().NonLazy();
                    Container.InstantiateComponent<CharacterHolder>(gameObject);
                    break;
                case PeerType.Client:
                    Container.Bind<IReadonlyHealth>().To<Health>().AsSingle();
                    Container.BindInterfacesTo<ClientCharacter>().AsSingle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Container.Bind<NetworkMovementData>().FromInstance(_movementData).AsSingle().NonLazy(); // triggers movement + lifetimed inject
        }

        private void BindHealth(INetPropertyFactory netFactory)
        {
            // Eager registration so VarId matches on client/server (lazy CreatePropertyFor skipped Health on remote clients).
            var healthProperty = netFactory.Viewable(
                _healthConfig.Max,
                NetworkDelivery.Reliable,
                NetworkPermissions.Server);

            Container.Bind<IViewableProperty<float>>()
                .FromInstance(healthProperty)
                .AsSingle()
                .WhenInjectedInto<Health>();
        }
    }
}
