using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Network.Prediction;
using Zenject;

namespace Bw.Injection.Network.Prediction
{
    public class PushTargetInstaller<TTarget, TEffect> : Installer<IRuntimeSettings, PushTargetInstaller<TTarget, TEffect>>
        where TTarget : class
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
                    Container.Bind<IEffectRules<MovementState, TEffect>>().To<PushEffectRules<TEffect>>().AsSingle();
                    Container.Bind<SimulationAffectable<MovementState, TEffect>>().AsSingle();
                    Container.Bind<AffectableRegistration<TTarget, SimulationAffectable<MovementState, TEffect>, TEffect>>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind(typeof(PredictedPush<TEffect>), typeof(IPredictedPush)).To<PredictedPush<TEffect>>().AsSingle();
                    Container.Bind<PredictionTargetRegistration<TTarget, PredictedPush<TEffect>, TEffect>>().AsSingle().NonLazy();
                    BindClientVisuals();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindClientVisuals() =>
            Container.Decorate<IStateView<MovementState>>().With<PushedStateView>();
    }
}
