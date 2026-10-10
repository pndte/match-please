using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Variables;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Network.Prediction
{
    public class EventPredictionTargetInstaller<TTarget, TState, TEffect>
        : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, EventPredictionTargetInstaller<TTarget, TState, TEffect>>
        where TTarget : class
        where TState : struct
        where TEffect : struct
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public EventPredictionTargetInstaller(
            IRuntimeSettings runtimeSettings,
            INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            var state = _netSchema.DeclareResult<CausedState<TState>>(NetworkDelivery.Reliable);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindResultSender(state).WhenInjectedInto(typeof(AuthoritativeState<TState, TEffect>), typeof(LateJoinStateSender<TState>));
                    Container.Bind<AuthoritativeState<TState, TEffect>>().AsSingle();
                    Container.Bind<LateJoinStateSender<TState>>().AsSingle().NonLazy();
                    Container.Bind<AffectableRegistration<TTarget, AuthoritativeState<TState, TEffect>, TEffect>>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.BindResultReceiver(state).WhenInjectedInto<PredictedState<TState, TEffect>>();
                    Container.Bind<PredictedState<TState, TEffect>>().AsSingle().NonLazy();
                    Container.Bind<PredictionTargetRegistration<TTarget, PredictedState<TState, TEffect>, TEffect>>().AsSingle().NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
