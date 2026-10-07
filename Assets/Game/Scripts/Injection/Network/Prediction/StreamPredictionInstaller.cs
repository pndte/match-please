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
            var requestedDeclaration = _netSchema.DeclareSignal<TickedInput<TInput>>(
                NetworkDelivery.Unreliable,
                NetworkPermissions.Client);
            var receivedDeclaration = _netSchema.DeclareSignal<TickedState<TState>>(
                NetworkDelivery.UnreliableSequenced,
                NetworkPermissions.Server);
            var inputMarginReceivedDeclaration = _netSchema.DeclareSignal<int>(
                NetworkDelivery.UnreliableSequenced,
                NetworkPermissions.Server);

            Container.BindInterfacesTo<PredictionSignals<TInput, TState>>()
                .FromMethod(context =>
                {
                    var entries = context.Container.Resolve<INetEntries>();
                    return new PredictionSignals<TInput, TState>(
                        entries.Get(requestedDeclaration),
                        entries.Get(receivedDeclaration),
                        entries.Get(inputMarginReceivedDeclaration));
                })
                .AsSingle();

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Client:
                    BindClient();
                    break;
                case PeerType.Server:
                    BindServer();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindClient()
        {
            Container.Bind<PredictionInputSender<TInput>>().AsSingle().NonLazy();
            Container.Bind<ISource<TInput>>()
                .FromResolveGetter<PredictionInputSender<TInput>>(sender => sender.Predicted)
                .AsSingle();

            Container.Bind<PredictionReconciler<TInput, TState>>().AsSingle().NonLazy();
            Container.Bind<InputMarginClientHandler>().AsSingle().NonLazy();
            Container.Bind<PredictionInterpolation<TState>>().AsSingle().NonLazy();
            Container.Bind<SnapshotInterpolation<TState>>().AsSingle().NonLazy();
        }

        private void BindServer()
        {
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
