using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Variables;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Network.Prediction
{
    public class WireRollbackStateInstaller<TState, TWire> : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, WireRollbackStateInstaller<TState, TWire>>
        where TState : struct, IEquatable<TState>
        where TWire : struct
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public WireRollbackStateInstaller(IRuntimeSettings runtimeSettings, INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            var state = _netSchema.DeclareResult<TickedState<TWire>>(NetworkDelivery.UnreliableSequenced);
            Container.Bind<IWireForm<TickedState<TState>, TickedState<TWire>>>().To<TickedStateForm<TState, TWire>>().AsSingle();
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Client:
                    Container.BindResultReceiver(state).WhenInjectedInto<WireResultReceiver<TickedState<TState>, TickedState<TWire>>>();
                    Container.Bind<INetResultReceiver<TickedState<TState>>>().To<WireResultReceiver<TickedState<TState>, TickedState<TWire>>>().AsSingle()
                        .WhenInjectedInto<ReplicatedStates<TState>>();
                    RollbackPartInstaller<TState>.Install(Container);
                    break;
                case PeerType.Server:
                    Container.BindResultSender(state).WhenInjectedInto<WireResultSender<TickedState<TState>, TickedState<TWire>>>();
                    Container.Bind<INetResultSender<TickedState<TState>>>().To<WireResultSender<TickedState<TState>, TickedState<TWire>>>().AsSingle()
                        .WhenInjectedInto<PredictionStateBroadcaster<TState>>();
                    Container.Bind<PredictionStateBroadcaster<TState>>().AsSingle().NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
