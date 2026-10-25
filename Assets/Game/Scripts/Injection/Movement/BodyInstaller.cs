using System;
using Bw.Entities.Network;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.Injection.Network;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Network.Prediction;
using Bw.UseCases.Movement.Physics;
using JetBrains.Lifetimes;
using UnityEngine.LowLevelPhysics2D;
using Zenject;

namespace Bw.Injection.Movement
{
    public class BodyInstaller<TState> : Installer<IRuntimeSettings, IBodyConfig, BodyInstaller<TState>>
        where TState : struct, IBodyState<TState>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly IBodyConfig _config;

        public BodyInstaller(IRuntimeSettings runtimeSettings, IBodyConfig config)
        {
            _runtimeSettings = runtimeSettings;
            _config = config;
        }

        public override void InstallBindings()
        {
            RequirePushDeceleration();

            Container.Bind<IBodyConfig>().FromInstance(_config).AsSingle();
            Container.Bind<PhysicsBody>().FromMethod(CreatePhysicsBody).AsSingle();
            Container.BindInterfacesTo<KinematicBody>().AsSingle();
            Container.Bind<PlatformRider>().AsSingle();
            Container.BindInterfacesTo<ShapeCollider>().AsSingle().WithArguments(_config.GroundLayer);
            Container.BindInterfacesTo<BodyMotor>().AsSingle();
            LagCompensationInstaller<TState, BodyPoseView<TState>>.Install(Container, _runtimeSettings);
            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
                BindClient();
        }

        private void BindClient()
        {
            Container.Bind<PredictedBodyPose<TState>>().AsSingle().NonLazy();
            Container.Decorate<IStateView<TState>>().With<KinematicFollowView<TState>>();
        }

        private static PhysicsBody CreatePhysicsBody(InjectContext context) =>
            context.Container.Resolve<IPhysicsSpace>().CreateBody(
                context.Container.Resolve<Lifetime>(),
                context.Container.Resolve<BodyBox>(),
                PhysicsBody.BodyType.Kinematic);

        private void RequirePushDeceleration()
        {
            if (_config.PushDeceleration <= 0f)
                throw new InvalidOperationException(
                    $"Body config '{_config}' has no push deceleration: a knockback would carry the body forever, and the shooter's predicted push would never meet the server's.");
        }
    }
}
