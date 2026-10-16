using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Variables;
using Bw.Entities.Physics;
using Bw.Entities.Simulation;
using Bw.Injection.Network;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Network.Platforms;
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
            Container.BindInterfacesTo<PlatformRider>().AsSingle();
            Container.Bind<IStateView<RigidState>>().To<RigidStateView>().AsSingle();

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    BindServer();
                    break;
                case PeerType.Client:
                    BindClientVisuals();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ReplicationInstaller<RigidState>.Install(Container, _runtimeSettings, _netSchema);
        }

        private void BindServer()
        {
            Container.Bind<PhysicsBody>().FromMethod(CreateBody).AsSingle();
            Container.Bind(typeof(IReadonlySimulation<RigidState>), typeof(ISimulation<RigidState>), typeof(IRestartable<RigidState>), typeof(IShovable))
                .To<DynamicBody>().AsSingle()
                .WriterOnlyInto<ISimulation<RigidState>>(typeof(SimulationAffectable<,>))
                .NonLazy();
        }

        private void BindClientVisuals() =>
            Container.Decorate<IStateView<RigidState>>().With<RiderStateView<RigidState>>();

        private PhysicsBody CreateBody(InjectContext context) =>
            _config.CreateBody(
                context.Container.Resolve<IPhysicsSpace>(),
                context.Container.Resolve<Lifetime>(),
                context.Container.Resolve<BoxCollider2D>(),
                PhysicsBody.BodyType.Dynamic);
    }
}
