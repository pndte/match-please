using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Variables;
using Zenject;

namespace Bw.Injection.Network.Prediction
{
    public class EventPredictionTargetInstaller<TTarget, TState, TEffect>
        : Installer<IRuntimeSettings, NetResultDeclaration<CausedState<TState>>, EventPredictionTargetInstaller<TTarget, TState, TEffect>>
        where TTarget : class
        where TState : struct
        where TEffect : struct
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly NetResultDeclaration<CausedState<TState>> _state;

        public EventPredictionTargetInstaller(IRuntimeSettings runtimeSettings, NetResultDeclaration<CausedState<TState>> state)
        {
            _runtimeSettings = runtimeSettings;
            _state = state;
        }

        public override void InstallBindings()
        {
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindResultSender(_state).WhenInjectedInto(typeof(AuthoritativeState<TState, TEffect>), typeof(LateJoinStateSender<TState>));
                    Container.Bind<AuthoritativeState<TState, TEffect>>().AsSingle();
                    Container.Bind<LateJoinStateSender<TState>>().AsSingle().NonLazy();
                    Container.Bind<AffectableRegistrar<TTarget, TEffect>>()
                        .FromMethod(context => context.Container.Instantiate<AffectableRegistrar<TTarget, TEffect>>(new object[] { context.Container.Resolve<AuthoritativeState<TState, TEffect>>() }))
                        .AsCached()
                        .NonLazy();
                    break;
                case PeerType.Client:
                    Container.BindResultReceiver(_state).WhenInjectedInto<PredictedState<TState, TEffect>>();
                    Container.Bind<PredictedState<TState, TEffect>>().AsSingle().NonLazy();
                    Container.Bind<PredictionTargetRegistrar<TTarget, TEffect>>()
                        .FromMethod(context => context.Container.Instantiate<PredictionTargetRegistrar<TTarget, TEffect>>(new object[] { context.Container.Resolve<PredictedState<TState, TEffect>>() }))
                        .AsCached()
                        .NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
