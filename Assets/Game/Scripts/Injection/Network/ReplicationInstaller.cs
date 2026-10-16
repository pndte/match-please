using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Network
{
    public class ReplicationInstaller<TState>
        : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, ReplicationInstaller<TState>>
        where TState : struct, IEquatable<TState>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public ReplicationInstaller(
            IRuntimeSettings runtimeSettings,
            INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            var state = _netSchema.DeclareResult<TickedState<TState>>(NetworkDelivery.ReliableSequenced);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindResultSender(state).WhenInjectedInto(typeof(StateChangeBroadcaster<TState>), typeof(LateJoinTickedStateSender<TState>));
                    Container.Bind<StateChangeBroadcaster<TState>>().AsSingle().NonLazy();
                    Container.Bind<LateJoinTickedStateSender<TState>>().AsSingle().NonLazy();
                    Container.Bind<SimulationViewSync<TState>>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.BindResultReceiver(state).WhenInjectedInto(typeof(SnapshotInterpolation<TState>), typeof(ReplicatedStates<TState>));
                    Container.Bind<SnapshotInterpolation<TState>>().AsSingle().NonLazy();
                    Container.Bind<IReplicatedStates<TState>>().To<ReplicatedStates<TState>>().AsSingle().NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
