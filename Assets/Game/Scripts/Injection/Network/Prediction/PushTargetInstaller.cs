using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
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
            Container.Bind<IEffectRules<MovementState, TEffect>>().To<PushEffectRules<MovementState, TEffect>>().AsSingle();
            Container.Bind<SimulationAffectable<MovementState, TEffect>>().AsSingle();

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<AffectableRegistration<TTarget, TEffect>>()
                        .FromMethod(context => context.Container.Instantiate<AffectableRegistration<TTarget, TEffect>>(new object[] { context.Container.Resolve<SimulationAffectable<MovementState, TEffect>>() }))
                        .AsCached()
                        .NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind<PredictedAffectableRegistration<TTarget, TEffect>>()
                        .FromMethod(context => context.Container.Instantiate<PredictedAffectableRegistration<TTarget, TEffect>>(new object[] { context.Container.Resolve<SimulationAffectable<MovementState, TEffect>>() }))
                        .AsCached()
                        .NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
