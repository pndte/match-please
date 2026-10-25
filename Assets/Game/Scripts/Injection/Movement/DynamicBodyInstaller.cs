using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Variables;
using Bw.Entities.Physics;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Network.Prediction;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Lifetimes;
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
            Container.Bind<IStateView<RigidState>>().To<RigidStateView>().AsSingle();
            Container.Bind(typeof(IReadonlySimulation<RigidState>), typeof(IRestorable<RigidState>), typeof(IRestartable<RigidState>), typeof(IShovable))
                .To<DynamicBody>().AsSingle()
                .WriterOnlyInto<IRestorable<RigidState>>(typeof(SimulationAffectable<,>), typeof(RollbackPart<RigidState>))
                .NonLazy();
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<PhysicsBody>().FromMethod(context => CreateBody(context, PhysicsBody.BodyType.Dynamic)).AsSingle();
                    break;
                case PeerType.Client:
                    Container.Bind<PhysicsBody>().FromMethod(context => CreateBody(context, PhysicsBody.BodyType.Kinematic)).AsSingle();
                    Container.Bind<PredictedBodyType>().AsSingle().NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            RigidStateInstaller.Install(Container, _runtimeSettings, _netSchema);
        }

        private PhysicsBody CreateBody(InjectContext context, PhysicsBody.BodyType type) =>
            _config.CreateBody(
                context.Container.Resolve<IPhysicsSpace>(),
                context.Container.Resolve<Lifetime>(),
                context.Container.Resolve<BodyBox>(),
                type);
    }
}
