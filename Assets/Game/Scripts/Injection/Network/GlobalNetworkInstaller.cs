using Bw.Entities.Network;
using Bw.Entities.Network.Ticks;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Network
{
    [CreateAssetMenu(fileName = "GlobalNetworkInstaller", menuName = "Installers/GlobalNetworkInstaller")]
    public class GlobalNetworkInstaller : ScriptableObjectInstaller<GlobalNetworkInstaller>
    {
        [SerializeField] private NetworkTicksConfig _ticksConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(_ticksConfig).AsSingle();

            MessageHandlersInstaller.Install(Container);
            NetworkTicksInstaller.Install(Container);
            Debug.Log($"[{nameof(GlobalNetworkInstaller)}]: Network Services Successfully Installed");
        }
    }
}
