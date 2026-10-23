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
    public class PredictionStateInstaller<TInput, TState> : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, PredictionStateInstaller<TInput, TState>>
        where TInput : struct
        where TState : struct, IEquatable<TState>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public PredictionStateInstaller(IRuntimeSettings runtimeSettings, INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            var state = _netSchema.DeclareResult<TickedState<TState>>(NetworkDelivery.UnreliableSequenced);

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

        private void BindClient(NetResultDeclaration<TickedState<TState>> state)
        {
            Container.BindResultReceiver(state).WhenInjectedInto(typeof(PredictionReconciler<TInput, TState>), typeof(SnapshotInterpolation<TState>), typeof(ReplicatedStates<TState>));

            Container.Bind(typeof(IStateCorrections<TState>), typeof(PredictionReconciler<TInput, TState>)).To<PredictionReconciler<TInput, TState>>().AsSingle().NonLazy();
            Container.Bind<PredictionInterpolation<TState>>().AsSingle().NonLazy();
            Container.Bind<SnapshotInterpolation<TState>>().AsSingle().NonLazy();
            Container.Bind<IReplicatedStates<TState>>().To<ReplicatedStates<TState>>().AsSingle();
        }

        private void BindServer(NetResultDeclaration<TickedState<TState>> state)
        {
            Container.BindResultSender(state).WhenInjectedInto<PredictionStateBroadcaster<TState>>();

            Container.Bind<PredictionStateBroadcaster<TState>>().AsSingle().NonLazy();
        }
    }
}
