using System;
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
        
        public override void InstallBindings()
        {
            Container.Bind<HealthConfig>().FromInstance(_healthConfig).AsSingle();
            Container.Bind<NetworkHealthData>().FromInstance(_networkHealthData).AsSingle();
            
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