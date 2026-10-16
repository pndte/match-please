using System;
using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Variables;
using Bw.Entities.Physics;
using Bw.Entities.Simulation;
using Bw.Injection.Network;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Network.Platforms;
using Bw.UseCases.Movement.Network.Prediction;
using Bw.UseCases.Movement.Physics;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Zenject;

namespace Bw.Injection.Movement
{
    public class PredictedBodyInstaller : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, DynamicBodyConfig, PredictedBodyInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;
        private readonly DynamicBodyConfig _config;

        public PredictedBodyInstaller(
            IRuntimeSettings runtimeSettings,
            INetEntriesSchemaBuilder netSchema,
            DynamicBodyConfig config)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.Bind<IMovementBody>().To<TransformBody>().AsSingle();
            Container.Bind<ShovableRegistration>().AsSingle().NonLazy();

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    BindServer();
                    break;
                case PeerType.Client:
                    BindClient();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            DynamicBodyInstaller.Install(Container, _runtimeSettings, _netSchema, _config);
        }

        private void BindServer()
        {
            LagCompensationInstaller<RigidState, RigidStateView>.Install(Container, _runtimeSettings);
            Container.Decorate<IRewindable>().With<RiderRewinder>();
        }

        private void BindClient()
        {
            Container.Bind<PhysicsBody>().FromMethod(CreateProxy).AsSingle();
            Container.Bind(typeof(IShovable), typeof(IPredictable), typeof(IPredictedBody), typeof(IPredictedRigidBody), typeof(IStateCorrections<RigidState>))
                .To<PredictedRigidBody>().AsSingle()
                .NonLazy();
            Container.Bind<PredictedBodyRegistration>().AsSingle().NonLazy();
            Container.Bind<PredictableRegistration>().AsSingle().NonLazy();
            BindClientVisuals();
        }

        private void BindClientVisuals()
        {
            Container.Decorate<IStateView<RigidState>>().With<SmoothedRigidStateView>();
            Container.Decorate<IStateView<RigidState>>().With<PredictedRigidStateView>();
        }

        private PhysicsBody CreateProxy(InjectContext context) =>
            _config.CreateBody(
                context.Container.Resolve<IPhysicsSpace>(),
                context.Container.Resolve<Lifetime>(),
                context.Container.Resolve<BoxCollider2D>(),
                PhysicsBody.BodyType.Kinematic);
    }
}
