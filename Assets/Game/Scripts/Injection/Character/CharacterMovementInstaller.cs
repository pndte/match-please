using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Network;
using Bw.Injection.Network.Prediction;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.View.Animation;
using Bw.UseCases.Movement.View.Audio;
using Bw.UseCases.Movement.Network;
using Bw.UseCases.Movement.Physics;
using Zenject;

namespace Bw.Injection
{
    public class CharacterMovementInstaller
        : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, CharacterMovementInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public CharacterMovementInstaller(
            IRuntimeSettings runtimeSettings,
            INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<CharacterBody>().AsSingle();
            Container.BindInterfacesTo<PlatformerMotor>().AsSingle();
            Container.Bind(typeof(IReadonlyMovement), typeof(IReadonlySimulation<MovementState>), typeof(ISimulation<MovementState>))
                .To<CharacterMovement>().AsSingle()
                .WriterOnlyInto<ISimulation<MovementState>>(typeof(PredictionReconciler<MovementInput, MovementState>), typeof(SimulationAffectable<,>))
                .NonLazy();
            Container.BindInterfacesTo<MovementInputSampler>().AsSingle();
            Container.BindInterfacesTo<MovementInputPolicy>().AsSingle();
            BindVisuals();

            StreamPredictionInstaller<MovementInput, MovementState>.Install(Container, _runtimeSettings, _netSchema);
            LagCompensationInstaller<MovementState, MovementStateView>.Install(Container, _runtimeSettings);
        }

        private void BindVisuals()
        {
            Container.BindInterfacesTo<SpriteMovementAnimator>().AsSingle();
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<IStateView<MovementState>>()
                        .FromMethod(context => new CompositeStateView<MovementState>(
                            context.Container.Instantiate<MovementStateView>(),
                            context.Container.Instantiate<MovementAnimationView>()))
                        .AsSingle();
                    break;
                case PeerType.Client:
                    Container.Bind<IStateView<MovementState>>()
                        .FromMethod(context => new CompositeStateView<MovementState>(
                            context.Container.Instantiate<MovementStateView>(),
                            context.Container.Instantiate<MovementAnimationView>(),
                            context.Container.Instantiate<MovementSoundsView>()))
                        .AsSingle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
