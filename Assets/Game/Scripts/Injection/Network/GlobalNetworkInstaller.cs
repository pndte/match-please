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
            MessageHandlersInstaller.Install(Container);
            Debug.Log($"[{nameof(GlobalNetworkInstaller)}]: Network Services Successfully Installed");
        }
    }
}