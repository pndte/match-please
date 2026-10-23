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
    public class WirePredictionStateInstaller<TInput, TState, TWire> : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, WirePredictionStateInstaller<TInput, TState, TWire>>
        where TInput : struct
        where TState : struct, IEquatable<TState>
        where TWire : struct
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public WirePredictionStateInstaller(IRuntimeSettings runtimeSettings, INetEntriesSchemaBuilder netSchema)
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
                    BindClient(state);
                    break;
                case PeerType.Server:
                    BindServer(state);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindClient(NetResultDeclaration<TickedState<TWire>> state)
        {
            Container.BindResultReceiver(state).WhenInjectedInto<WireResultReceiver<TickedState<TState>, TickedState<TWire>>>();
            Container.Bind<INetResultReceiver<TickedState<TState>>>().To<WireResultReceiver<TickedState<TState>, TickedState<TWire>>>().AsSingle()
                .WhenInjectedInto(typeof(PredictionReconciler<TInput, TState>), typeof(SnapshotInterpolation<TState>), typeof(ReplicatedStates<TState>));

            Container.Bind(typeof(IStateCorrections<TState>), typeof(PredictionReconciler<TInput, TState>)).To<PredictionReconciler<TInput, TState>>().AsSingle().NonLazy();
            Container.Bind<PredictionInterpolation<TState>>().AsSingle().NonLazy();
            Container.Bind<SnapshotInterpolation<TState>>().AsSingle().NonLazy();
            Container.Bind<IReplicatedStates<TState>>().To<ReplicatedStates<TState>>().AsSingle();
        }

        private void BindServer(NetResultDeclaration<TickedState<TWire>> state)
        {
            Container.BindResultSender(state).WhenInjectedInto<WireResultSender<TickedState<TState>, TickedState<TWire>>>();
            Container.Bind<INetResultSender<TickedState<TState>>>().To<WireResultSender<TickedState<TState>, TickedState<TWire>>>().AsSingle()
                .WhenInjectedInto<PredictionStateBroadcaster<TState>>();

            Container.Bind<PredictionStateBroadcaster<TState>>().AsSingle().NonLazy();
        }
    }
}
