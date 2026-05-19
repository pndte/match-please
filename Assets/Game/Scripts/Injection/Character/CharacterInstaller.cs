using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Injection.Network;
using Bw.Injection.Network.Variables;
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
        [SerializeField] private NetworkLifetimedBehaviour _networkLifetimedBehaviour;
        [SerializeField] private Rigidbody2D _physics;
        [SerializeField] private MovementConfig _movementConfig;
        [SerializeField] private NetworkMovementData _movementData;

        public override void InstallBindings()
        {
            NetTablesInstaller.Install(Container, _networkObject);

            Container.Bind<HealthConfig>().FromInstance(_healthConfig).AsSingle();

            Container.Bind<Rigidbody2D>().To<Rigidbody2D>().FromInstance(_physics).AsSingle();
            Container.Bind<MovementConfig>().To<MovementConfig>().FromInstance(_movementConfig).AsSingle();

            var gameObjectLifetime = gameObject.Lifetime();
            Container.BindInstance(gameObjectLifetime).AsSingle();
            Container.BindInterfacesTo<NetworkLifetimedBehaviour>().FromInstance(_networkLifetimedBehaviour).AsSingle();
            Container.CreatePropertyFor<float, Health>(_healthConfig.Max);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<NetworkObject>().FromInstance(_networkObject).AsSingle();
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

            Container.Bind<NetworkMovementData>().FromInstance(_movementData).AsSingle().NonLazy();
        }
    }
}