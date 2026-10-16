using System;
using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Variables;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
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
    public class DynamicBodyInstaller : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, DynamicBodyConfig, DynamicBodyInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;
        private readonly DynamicBodyConfig _config;

        public DynamicBodyInstaller(
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
            Container.BindInstance(_config).AsSingle();
            Container.Bind<IMovementBody>().To<TransformBody>().AsSingle();
            Container.Bind<IPlatformRider>().To<ShapeCollider>().AsSingle().WithArguments(_config.Contacts);
            Container.Bind<IStateView<RigidState>>().To<RigidStateView>().AsSingle();
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

            ReplicationInstaller<RigidState>.Install(Container, _runtimeSettings, _netSchema);
        }

        private void BindServer()
        {
            Container.Bind<PhysicsBody>().FromMethod(context => CreateBody(context, _config.BodyDefinition())).AsSingle();
            Container.Bind(typeof(IReadonlySimulation<RigidState>), typeof(ISimulation<RigidState>), typeof(IRestartable<RigidState>), typeof(IShovable))
                .To<DynamicBody>().AsSingle()
                .WriterOnlyInto<ISimulation<RigidState>>(typeof(SimulationAffectable<,>))
                .NonLazy();
            LagCompensationInstaller<RigidState, RigidStateView>.Install(Container, _runtimeSettings);
            Container.Decorate<IRewindable>().With<RiderRewinder>();
        }

        private void BindClient()
        {
            Container.Bind<PhysicsBody>().FromMethod(context => CreateBody(context, ProxyDefinition())).AsSingle();
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
            Container.Decorate<IStateView<RigidState>>().With<RiderStateView<RigidState>>();
        }

        private PhysicsBody CreateBody(InjectContext context, PhysicsBodyDefinition definition)
        {
            var box = context.Container.Resolve<BoxCollider2D>();
            var body = context.Container.Resolve<IPhysicsSpace>().CreateBody(context.Container.Resolve<Lifetime>(), box, definition, _config.ShapeDefinition());
            body.SweepContinuously(box.WorldOffset());
            return body;
        }

        private PhysicsBodyDefinition ProxyDefinition()
        {
            var body = _config.BodyDefinition();
            body.type = PhysicsBody.BodyType.Kinematic;
            return body;
        }
    }
}
