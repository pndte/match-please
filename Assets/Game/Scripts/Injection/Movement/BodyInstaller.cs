using System;
using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.Injection.Network;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Network.Platforms;
using Bw.UseCases.Movement.Network.Prediction;
using Bw.UseCases.Movement.Physics;
using JetBrains.Lifetimes;
using UnityEngine;
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
            Container.BindInterfacesAndSelfTo<PlatformRider>().AsSingle();
            Container.BindInterfacesTo<ShapeCollider>().AsSingle().WithArguments(_config.GroundLayer);
            Container.BindInterfacesTo<BodyMotor>().AsSingle();
            LagCompensationInstaller<TState, BodyStateView<TState>>.Install(Container, _runtimeSettings);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Decorate<IRewindable>().With<RiderRewinder>();
                    break;
                case PeerType.Client:
                    Container.Decorate<IStateView<TState>>().With<RiderStateView<TState>>();
                    Container.Bind<ControlledBody>().AsSingle().NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static PhysicsBody CreatePhysicsBody(InjectContext context) =>
            context.Container.Resolve<IPhysicsSpace>().CreateBody(
                context.Container.Resolve<Lifetime>(),
                context.Container.Resolve<BoxCollider2D>(),
                PhysicsBody.BodyType.Kinematic);

        private void RequirePushDeceleration()
        {
            if (_config.PushDeceleration <= 0f)
                throw new InvalidOperationException(
                    $"Body config '{_config}' has no push deceleration: a knockback would carry the body forever, and the shooter's predicted push would never meet the server's.");
        }
    }
}
