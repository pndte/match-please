using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Network.Prediction;
using Zenject;

namespace Bw.Injection.Network.Prediction
{
    public class PushTargetInstaller<TTarget, TState, TEffect> : Installer<IRuntimeSettings, PushTargetInstaller<TTarget, TState, TEffect>>
        where TTarget : class
        where TState : struct, IBodyState<TState>
        where TEffect : struct
    {
        private readonly IRuntimeSettings _runtimeSettings;

        public PushTargetInstaller(IRuntimeSettings runtimeSettings)
        {
            _runtimeSettings = runtimeSettings;
        }

        public override void InstallBindings()
        {
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<IEffectRules<TState, TEffect>>().To<PushEffectRules<TState, TEffect>>().AsSingle();
                    Container.Bind<SimulationAffectable<TState, TEffect>>().AsSingle();
                    Container.Bind<IAffectable<TEffect>>().To<SimulationAffectable<TState, TEffect>>().FromResolve()
                        .WhenInjectedInto<AffectableRegistration<TTarget, SimulationAffectable<TState, TEffect>, TEffect>>();
                    Container.Bind<AffectableRegistration<TTarget, SimulationAffectable<TState, TEffect>, TEffect>>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind(typeof(PredictedPush<TEffect>), typeof(IPredictedPush)).To<PredictedPush<TEffect>>().AsSingle();
                    Container.Bind<IPredictionTarget<TEffect>>().To<PredictedPush<TEffect>>().FromResolve()
                        .WhenInjectedInto<PredictionTargetRegistration<TTarget, PredictedPush<TEffect>, TEffect>>();
                    Container.Bind<PredictionTargetRegistration<TTarget, PredictedPush<TEffect>, TEffect>>().AsSingle().NonLazy();
                    BindClientVisuals();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindClientVisuals() =>
            Container.Decorate<IStateView<TState>>().With<PushedStateView<TState>>();
    }
}
