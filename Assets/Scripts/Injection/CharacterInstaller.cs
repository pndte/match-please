using System;
using DefaultNamespace;
using Entities;
using Entities.Network;
using JetBrains.Lifetimes;
using Setup;
using UnityEngine;
using UseCases.Network;
using Zenject;

namespace Injection
{
    public class CharacterInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [SerializeField] private NetworkLifetimedBehaviour _networkLifetimedBehaviour;
        [SerializeField] private NetworkCharacter _networkCharacter;
        
        [SerializeField] private HealthConfig _healthConfig;
        [SerializeField] private NetworkHealthData _networkHealthData;
        
        [SerializeField] private Rigidbody2D _physics;
        [SerializeField] private MovementConfig _movementConfig;
        
        public override void InstallBindings()
        {
            Container.Bind<HealthConfig>().FromInstance(_healthConfig).AsSingle();
            Container.Bind<NetworkHealthData>().FromInstance(_networkHealthData).AsSingle();

            Container.Bind<Rigidbody2D>().To<Rigidbody2D>().FromInstance(_physics).AsSingle();
            Container.Bind<MovementConfig>().To<MovementConfig>().FromInstance(_movementConfig).AsSingle();
            
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindInterfacesAndSelfTo<NetworkLifetimedBehaviour>().FromInstance(_networkLifetimedBehaviour).AsSingle();
                    Container.BindInterfacesTo<NetworkCharacter>().FromInstance(_networkCharacter).AsSingle();
                    Container.BindInterfacesAndSelfTo<NetworkHealth>().AsSingle();
                    Container.Bind<HealthDataConnector>().AsSingle().NonLazy();
                    Container.Bind<DamageProcessor>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind<IReadonlyHealth>().To<NetworkHealth>().AsSingle();
                    Container.Bind<IReadonlyCharacter>().To<NetworkCharacter>().FromInstance(_networkCharacter).AsSingle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}