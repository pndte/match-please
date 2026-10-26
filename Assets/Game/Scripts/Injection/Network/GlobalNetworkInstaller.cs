using Bw.Entities.Extensions;
using Bw.Injection.Network.Prediction;
using Bw.UseCases;
using Bw.UseCases.Clients.Network;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Network
{
    [CreateAssetMenu(fileName = "GlobalNetworkInstaller", menuName = "Installers/GlobalNetworkInstaller")]
    public class GlobalNetworkInstaller : ScriptableObjectInstaller<GlobalNetworkInstaller>
    {
        public override void InstallBindings()
        {
            BindSession();
            MessageHandlersInstaller.Install(Container);
            NetworkTicksInstaller.Install(Container);
            EventPredictionInstaller.Install(Container);
            Debug.Log($"[{nameof(GlobalNetworkInstaller)}]: Network Services Successfully Installed");
        }

        private void BindSession()
        {
            Container.Bind<Lifetime>().FromMethod(() => Container.Resolve<Context>().gameObject.Lifetime()).AsSingle();
            Container.BindInterfacesTo<NetworkClientCollection>().AsSingle().WithArguments(NetworkManager.Singleton).NonLazy();
            Container.BindInterfacesTo<CharacterRegistry>().AsSingle();
        }
    }
}
