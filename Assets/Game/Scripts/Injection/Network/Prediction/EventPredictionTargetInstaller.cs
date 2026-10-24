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
                    Container.Bind<AffectableRegistration<TTarget, TEffect>>()
                        .FromMethod(context => context.Container.Instantiate<AffectableRegistration<TTarget, TEffect>>(new object[] { context.Container.Resolve<AuthoritativeState<TState, TEffect>>() }))
                        .AsCached()
                        .NonLazy();
                    break;
                case PeerType.Client:
                    Container.BindResultReceiver(_state).WhenInjectedInto<PredictedState<TState, TEffect>>(); //TODO: состояние цели показывается, как только пришло, а чужой выстрел, который его вызвал, клиент рисует на тик интерполяции позже: держатель видит, как разбился ящик и умер он сам, раньше, чем в него прилетела пуля
                    Container.Bind<PredictedState<TState, TEffect>>().AsSingle().NonLazy();
                    Container.Bind<PredictionTargetRegistration<TTarget, TEffect>>()
                        .FromMethod(context => context.Container.Instantiate<PredictionTargetRegistration<TTarget, TEffect>>(new object[] { context.Container.Resolve<PredictedState<TState, TEffect>>() }))
                        .AsCached()
                        .NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
