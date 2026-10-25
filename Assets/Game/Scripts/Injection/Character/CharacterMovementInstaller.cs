using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Movement;
using Bw.Injection.Network.Prediction;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Network;
using Bw.UseCases.Movement.Network.Prediction;
using Bw.UseCases.Movement.Network.Requests;
using Bw.UseCases.Movement.Physics;
using Bw.UseCases.Movement.View;
using Bw.UseCases.Movement.View.Adapters;
using Bw.UseCases.Movement.View.Animation;
using Bw.UseCases.Movement.View.Animation.Abstractions;
using Bw.UseCases.Movement.View.Audio;
using Zenject;

namespace Bw.Injection
{
    public class CharacterMovementInstaller
        : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, MovementConfig, CharacterMovementInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;
        private readonly MovementConfig _config;

        public CharacterMovementInstaller(
            IRuntimeSettings runtimeSettings,
            INetEntriesSchemaBuilder netSchema,
            MovementConfig config)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.Bind<MovementConfig>().FromInstance(_config).AsSingle();
            BodyInstaller<MovementState>.Install(Container, _runtimeSettings, _config);
            Container.BindInterfacesTo<PlatformerMotor>().AsSingle();
            Container.Bind(typeof(IReadonlyMovement), typeof(IReadonlySimulation<MovementState>), typeof(IRestorable<MovementState>), typeof(ISteppable<MovementInput>))
                .To<CharacterMovement>().AsSingle()
                .WriterOnlyInto<IRestorable<MovementState>>(typeof(RollbackPart<MovementState>), typeof(ServerStateFollower<MovementState>), typeof(SimulationAffectable<,>))
                .NonLazy();
            BindControls();
            BindVisuals();

            RollbackStateInstaller<MovementState>.Install(Container, _runtimeSettings, _netSchema);
        }

        private void BindControls()
        {
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<IInputPolicy<MovementControls>>().To<MovementControlsPolicy>().AsSingle();
                    break;
                case PeerType.Client:
                    Container.Bind<IInputSampler<MovementControls>>().To<MovementControlsSampler>().AsSingle();
                    Container.Bind<IStateDivergence<MovementState>>().To<MovementDivergence>().AsSingle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindVisuals()
        {
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<IStateView<MovementState>>()
                        .FromMethod(context => new CompositeStateView<MovementState>(
                            context.Container.Instantiate<BodyPoseView<MovementState>>(),
                            MotionOf(context.Container, context.Container.Instantiate<MovementAnimationView>())))
                        .AsSingle();
                    BindFacing();
                    Container.Bind<SimulationViewSync<MovementState>>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind<IStateView<MovementState>>()
                        .FromMethod(context => new CompositeStateView<MovementState>(
                            context.Container.Instantiate<BodyStateView<MovementState>>(),
                            MotionOf(
                                context.Container,
                                context.Container.Instantiate<MovementAnimationView>(),
                                context.Container.Instantiate<MovementSoundsView>())))
                        .AsSingle();
                    Container.Decorate<IStateView<MovementState>>().With<SmoothedStateView<MovementState>>();
                    Container.Decorate<IStateView<MovementState>>().With<TimelineBlendView<MovementState>>();
                    BindFacing();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindFacing()
        {
            Container.Bind(typeof(IFacingViewModel), typeof(RunFacing)).To<RunFacing>().AsSingle();
            Container.Decorate<IStateView<MovementState>>().With<RunFacingView>();
        }

        private static IStateView<MovementState> MotionOf(DiContainer container, params IStateView<MovementMotion>[] views) =>
            container.Instantiate<MovementMotionViewModel>(new object[] { new CompositeStateView<MovementMotion>(views) });
    }
}
