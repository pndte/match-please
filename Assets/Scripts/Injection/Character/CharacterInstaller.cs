using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.UseCases.Character;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Network;
using Setup;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public class CharacterInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [SerializeField] private NetworkLifetimedBehaviour _networkLifetimedBehaviour;

        [SerializeField] private HealthConfig _healthConfig;
        [SerializeField] private NetworkHealthData _networkHealthData;

        [SerializeField] private Rigidbody2D _physics;
        [SerializeField] private MovementConfig _movementConfig;
        [SerializeField] private NetworkMovementData _movementData;

        public override void InstallBindings()
        {
            Container.Bind<HealthConfig>().FromInstance(_healthConfig).AsSingle();
            Container.Bind<NetworkHealthData>().FromInstance(_networkHealthData).AsSingle();

            Container.Bind<Rigidbody2D>().To<Rigidbody2D>().FromInstance(_physics).AsSingle();
            Container.Bind<MovementConfig>().To<MovementConfig>().FromInstance(_movementConfig).AsSingle();

            var gameObjectLifetime = gameObject.Lifetime();
            Container.BindInstance(gameObjectLifetime).AsSingle();
            Container.BindInterfacesTo<NetworkLifetimedBehaviour>().FromInstance(_networkLifetimedBehaviour).AsSingle();

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<NetworkObject>().FromInstance(_networkLifetimedBehaviour.NetworkObject).AsSingle(); // TODO: remove, сделать интерфейс для этого свой
                    Container.BindInterfacesTo<ServerCharacter>().AsSingle();
                    Container.BindInterfacesAndSelfTo<NetworkHealth>().AsSingle();
                    Container.Bind<HealthDataConnector>().AsSingle().NonLazy();
                    Container.Bind<DamageProcessor>().AsSingle().NonLazy();
                    Container.InstantiateComponent<CharacterHolder>(gameObject);
                    break;
                case PeerType.Client:
                    Container.Bind<IReadonlyHealth>().To<NetworkHealth>().AsSingle();
                    Container.BindInterfacesTo<ClientCharacter>().AsSingle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Container.Bind<NetworkMovementData>().FromInstance(_movementData).AsSingle().NonLazy();
        }
    }
}