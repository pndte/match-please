using Bw.Entities.Network;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Network
{
    [CreateAssetMenu(fileName = "GlobalNetworkInstaller", menuName = "Installers/GlobalNetworkInstaller")]
    public class GlobalNetworkInstaller : ScriptableObjectInstaller<GlobalNetworkInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NetworkHolder>().AsSingle();
            MessageHandlersInstaller.Install(Container);
        }
    }
}