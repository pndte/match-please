using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Network;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.View.Animation;
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
            Container.BindInterfacesTo<CharacterMovement>().AsSingle().NonLazy();
            Container.BindInterfacesTo<SpriteMovementAnimator>().AsSingle();
            Container.Bind<IStateView<MovementState>>()
                .FromMethod(context => new CompositeStateView<MovementState>(
                    context.Container.Instantiate<MovementStateView>(),
                    context.Container.Instantiate<MovementAnimationView>()))
                .AsSingle();
            Container.BindInterfacesTo<MovementInputSampler>().AsSingle();
            Container.BindInterfacesTo<MovementInputPolicy>().AsSingle();

            PredictionInstaller<MovementInput, MovementState>.Install(Container, _runtimeSettings, _netSchema);
            LagCompensationInstaller<MovementState, MovementStateView>.Install(Container, _runtimeSettings);
        }
    }
}
