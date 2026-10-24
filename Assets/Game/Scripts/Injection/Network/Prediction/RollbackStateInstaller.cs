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
    public class RollbackStateInstaller<TState> : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, RollbackStateInstaller<TState>>
        where TState : struct, IEquatable<TState>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public RollbackStateInstaller(IRuntimeSettings runtimeSettings, INetEntriesSchemaBuilder netSchema)
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
                    Container.BindResultReceiver(state).WhenInjectedInto<ReplicatedStates<TState>>();
                    RollbackPartInstaller<TState>.Install(Container);
                    InterpolationInstaller<TState>.Install(Container);
                    break;
                case PeerType.Server:
                    Container.BindResultSender(state).WhenInjectedInto<PredictionStateBroadcaster<TState>>();
                    Container.Bind<PredictionStateBroadcaster<TState>>().AsSingle().NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
