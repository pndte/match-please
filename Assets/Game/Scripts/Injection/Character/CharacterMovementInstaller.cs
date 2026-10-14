using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Movement;
using Bw.Injection.Network.Prediction;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.View;
using Bw.UseCases.Movement.View.Adapters;
using Bw.UseCases.Movement.View.Animation;
using Bw.UseCases.Movement.View.Audio;
using Bw.UseCases.Movement.Network;
using Bw.UseCases.Movement.Network.Root;
using Bw.UseCases.Movement.Physics;
using JetBrains.Collections.Viewable;
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
            Container.Bind(typeof(IReadonlyMovement), typeof(IReadonlySimulation<MovementState>), typeof(ISimulation<MovementState>))
                .To<CharacterMovement>().AsSingle()
                .WriterOnlyInto<ISimulation<MovementState>>(typeof(PredictionReconciler<MovementInput, MovementState>), typeof(SimulationAffectable<,>))
                .NonLazy();
            Container.BindInterfacesTo<MovementInputSampler>().AsSingle();
            Container.BindInterfacesTo<MovementInputPolicy>().AsSingle();
            BindRoot();
            BindVisuals();

            StreamPredictionInstaller<MovementInput, MovementState>.Install(Container, _runtimeSettings, _netSchema);
        }

        private void BindRoot()
        {
            Container.Bind(typeof(IReadonlyMovementRoot), typeof(IMovementRoot)).To<MovementRoot>().AsSingle()
                .WriterOnlyInto<IMovementRoot>(typeof(MovementRootHolder));

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Decorate<ISource<MovementInput>>().With<RootedMovementInputs>();
                    break;
                case PeerType.Client:
                    Container.Decorate<IInputSampler<MovementInput>>().With<RootedMovementSampler>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindVisuals()
        {
            Container.BindInterfacesTo<SpriteMovementAnimator>().AsSingle();
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<IStateView<MovementState>>()
                        .FromMethod(context => new CompositeStateView<MovementState>(
                            context.Container.Instantiate<BodyStateView<MovementState>>(),
                            MotionOf(context.Container, context.Container.Instantiate<MovementAnimationView>())))
                        .AsSingle();
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
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IStateView<MovementState> MotionOf(DiContainer container, params IStateView<MovementMotion>[] views) =>
            container.Instantiate<MovementMotionViewModel>(new object[] { new CompositeStateView<MovementMotion>(views) });
    }
}
