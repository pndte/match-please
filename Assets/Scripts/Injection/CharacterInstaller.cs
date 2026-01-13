using Entities;
using Entities.Network;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Injection
{
    public class CharacterInstaller : MonoInstaller
    {
        [SerializeField] private HealthConfig _healthConfig;
        [FormerlySerializedAs("_healthData")] [SerializeField] private NetworkHealthData _networkHealthData;
        
        public override void InstallBindings()
        {
            Container.Bind<HealthConfig>().FromInstance(_healthConfig).AsSingle();
            Container.Bind<NetworkHealthData>().FromInstance(_networkHealthData).AsSingle();
            Container.BindInterfacesTo<NetworkHealth>().AsSingle().WithArguments(Lifetime.Eternal, _healthConfig, _networkHealthData);
        }
    }
}