using Bw.Entities.Network;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
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
                    Container.Bind(typeof(INetworkTicks), typeof(ISimulationStep), typeof(IWorldClock))
                        .To<ServerNetworkTicks>()
                        .AsSingle()
                        .WriterOnlyInto<IWorldClock>(typeof(WorldStep))
                        .NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind(
                            typeof(INetworkTicks),
                            typeof(ISimulationStep),
                            typeof(IWorldClock),
                            typeof(IInterpolationTicks),
                            typeof(IServerTime),
                            typeof(IInputMarginFeedback),
                            typeof(IInputLead))
                        .To<ClientNetworkTicks>()
                        .AsSingle()
                        .WriterOnlyInto<IWorldClock>(typeof(RollbackStepper))
                        .NonLazy();
                    break;
            }
        }
    }
}
