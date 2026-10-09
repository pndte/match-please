using Bw.Entities.Network;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using JetBrains.Lifetimes;
using Zenject;

namespace Bw.Injection.Network
{
    public class NetworkTicksInstaller : Installer<NetworkTicksInstaller>
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        public override void InstallBindings()
        {
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind(typeof(INetworkTicks), typeof(ISimulationStep))
                        .To<ServerNetworkTicks>()
                        .AsSingle()
                        .WithArguments(Lifetime.Eternal)
                        .NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind(
                            typeof(INetworkTicks),
                            typeof(ISimulationStep),
                            typeof(IInterpolationTicks),
                            typeof(IServerTime),
                            typeof(IInputMarginFeedback))
                        .To<ClientNetworkTicks>()
                        .AsSingle()
                        .WithArguments(Lifetime.Eternal)
                        .NonLazy();
                    break;
            }
        }
    }
}
