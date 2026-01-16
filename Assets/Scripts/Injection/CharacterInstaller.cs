using System;
using DefaultNamespace;
using Entities;
using Entities.Network;
using JetBrains.Lifetimes;
using Setup;
using UnityEngine;
using Zenject;

namespace Injection
{
    public class CharacterInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;
        
        [SerializeField] private HealthConfig _healthConfig;
        [SerializeField] private NetworkHealthData _networkHealthData;
        
        [SerializeField] private Rigidbody2D _physics;
        [SerializeField] private MovementConfig _movementConfig;
        [SerializeField] private NetworkMovementData _networkMovementData;
        
        public override void InstallBindings()
        {
            Container.Bind<HealthConfig>().FromInstance(_healthConfig).AsSingle();
            Container.Bind<NetworkHealthData>().FromInstance(_networkHealthData).AsSingle();

            Container.Bind<Rigidbody2D>().To<Rigidbody2D>().FromInstance(_physics).AsSingle();
            Container.Bind<MovementConfig>().To<MovementConfig>().FromInstance(_movementConfig).AsSingle();
            Container.Bind<NetworkMovementData>().FromInstance(_networkMovementData).AsSingle().NonLazy();
            
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindInterfacesAndSelfTo<NetworkHealth>().AsSingle().WithArguments(Lifetime.Eternal);
                    Container.Bind<HealthDataConnector>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind<IReadonlyHealth>().To<NetworkHealth>().AsSingle().WithArguments(Lifetime.Eternal);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}