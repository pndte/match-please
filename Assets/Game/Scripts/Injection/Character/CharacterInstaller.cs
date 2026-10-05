using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using Bw.Injection.ControlledBy;
using Bw.Injection.Network;
using Bw.Injection.Network.Variables;
using JetBrains.Collections.Viewable;
using Bw.Injection.Ownership;
using Bw.UseCases;
using Bw.UseCases.Character;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Movement;
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
        [SerializeField] private NetworkLifetimedBehaviour _networkLifetimedBehaviour;
        [SerializeField] private Rigidbody2D _physics;
        [SerializeField] private BoxCollider2D _collider;
        [SerializeField] private MovementConfig _movementConfig;
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _sprite;

        public override void InstallBindings()
        {
            Debug.Log("Character installer executed");

            OwnershipInstaller.Install(Container, _runtimeSettings);
            ControlledByInstaller.Install(Container, _runtimeSettings);
            
            Container.Bind<NetworkObject>().FromInstance(_networkObject).AsSingle();
            Container.Bind<INetworkLifetimedObject>().FromInstance(_networkLifetimedBehaviour).AsSingle().NonLazy();
            var gameObjectLifetime = gameObject.Lifetime();
            Container.BindInstance(gameObjectLifetime).AsSingle();

            Container.Bind<HealthConfig>().FromInstance(_healthConfig).AsSingle();
            Container.Bind<Rigidbody2D>().FromInstance(_physics).AsSingle();
            Container.Bind<BoxCollider2D>().FromInstance(_collider).AsSingle();
            Container.Bind<MovementConfig>().FromInstance(_movementConfig).AsSingle();
            Container.Bind<Animator>().FromInstance(_animator).AsSingle();
            Container.Bind<SpriteRenderer>().FromInstance(_sprite).AsSingle();

            var netSchema = new NetEntriesSchemaBuilder();
            OwnershipServicesInstaller.Install(Container, _runtimeSettings, netSchema);
            ControlledByServicesInstaller.Install(Container, _runtimeSettings, netSchema);
            CharacterMovementInstaller.Install(Container, _runtimeSettings, netSchema);
            var healthDeclaration = netSchema.DeclareProperty(_healthConfig.Max, NetworkDelivery.Reliable, NetworkPermissions.Server);
            Container.BindNetPropertyFor<float, Health>(healthDeclaration);
            NetTablesInstaller.Install(Container, _runtimeSettings, netSchema.Build());

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindInterfacesTo<ServerCharacter>().AsSingle();
                    Container.BindInterfacesAndSelfTo<Health>().AsSingle().NonLazy();
                    Container.Bind<DamageProcessor>().AsSingle().NonLazy();
                    Container.InstantiateComponent<CharacterHolder>(gameObject);
                    break;
                case PeerType.Client:
                    Container.Bind<IReadonlyHealth>().To<Health>().AsSingle().NonLazy();
                    Container.BindInterfacesTo<ClientCharacter>().AsSingle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
