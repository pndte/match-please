using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Network.Prediction
{
    public class StreamPredictionInstaller<TInput, TState>
        : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, StreamPredictionInstaller<TInput, TState>>
        where TInput : struct
        where TState : struct
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public StreamPredictionInstaller(
            IRuntimeSettings runtimeSettings,
            INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            var input = _netSchema.DeclareRequest<TickedInput<TInput>>(NetworkDelivery.Unreliable);
            var state = _netSchema.DeclareResult<TickedState<TState>>(NetworkDelivery.UnreliableSequenced);
            var inputMargin = _netSchema.DeclareResult<int>(NetworkDelivery.UnreliableSequenced);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Client:
                    BindClient(input, state, inputMargin);
                    break;
                case PeerType.Server:
                    BindServer(input, state, inputMargin);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindClient(
            NetRequestDeclaration<TickedInput<TInput>> input,
            NetResultDeclaration<TickedState<TState>> state,
            NetResultDeclaration<int> inputMargin)
        {
            Container.BindRequestSender(input).WhenInjectedInto(typeof(PredictionInputSender<TInput>), typeof(PredictionReconciler<TInput, TState>));
            Container.BindResultReceiver(state).WhenInjectedInto(typeof(PredictionReconciler<TInput, TState>), typeof(SnapshotInterpolation<TState>));
            Container.BindResultReceiver(inputMargin).WhenInjectedInto<InputMarginClientHandler>();

            Container.Bind<PredictionInputSender<TInput>>().AsSingle().NonLazy();
            Container.Bind<ISource<TInput>>()
                .FromResolveGetter<PredictionInputSender<TInput>>(sender => sender.Predicted)
                .AsSingle();

            Container.Bind<PredictionReconciler<TInput, TState>>().AsSingle().NonLazy();
            Container.Bind<InputMarginClientHandler>().AsSingle().NonLazy();
            Container.Bind<PredictionInterpolation<TState>>().AsSingle().NonLazy();
            Container.Bind<SnapshotInterpolation<TState>>().AsSingle().NonLazy();
        }

        private void BindServer(
            NetRequestDeclaration<TickedInput<TInput>> input,
            NetResultDeclaration<TickedState<TState>> state,
            NetResultDeclaration<int> inputMargin)
        {
            Container.BindRequestReceiver(input).WhenInjectedInto(typeof(PredictionInputBuffer<TInput>), typeof(InputMarginServerHandler<TInput>));
            Container.BindResultSender(state).WhenInjectedInto<PredictionStateBroadcaster<TState>>();
            Container.BindResultSender(inputMargin).WhenInjectedInto<InputMarginServerHandler<TInput>>();

            Container.Bind<PredictionInputBuffer<TInput>>().AsSingle().NonLazy();
            Container.Bind<ISource<TInput>>()
                .FromResolveGetter<PredictionInputBuffer<TInput>>(buffer => buffer.Simulated)
                .AsSingle();

            Container.Bind<PredictionStateBroadcaster<TState>>().AsSingle().NonLazy();
            Container.Bind<InputMarginServerHandler<TInput>>().AsSingle().NonLazy();
            Container.Bind<SimulationViewSync<TState>>().AsSingle().NonLazy();
        }
    }
}
