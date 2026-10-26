using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Network;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Network.Prediction;
using Bw.UseCases.Movement.Physics;
using Bw.UseCases.Movement.Physics.Abstractions;
using Zenject;

namespace Bw.Injection.Movement
{
    public sealed class RigidStateInstaller : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, RigidStateInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public RigidStateInstaller(IRuntimeSettings runtimeSettings, INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            ReplicationInstaller<RigidState>.Install(Container, _runtimeSettings, _netSchema);
            if (_runtimeSettings.CurrentPeerType != PeerType.Client)
                return;

            Container.Bind<IStateDivergence<RigidState>>().To<RigidDivergence>().AsSingle();
            Container.Bind<RollbackPart<RigidState>>().AsSingle();
            Container.Bind<IStateCorrections<RigidState>>().To<StateCorrections<RigidState>>().AsSingle();
            Container.Bind<RollbackPartRegistrar<RigidState>>().AsSingle().NonLazy();
            Container.Bind<PredictionInterpolator<RigidState>>().AsSingle().NonLazy();
            BindClientVisuals();
        }

        private void BindClientVisuals()
        {
            Container.Bind<IBodyPose>().To<PhysicsBodyPose>().AsSingle();
            Container.Decorate<IStateView<RigidState>>().With<SmoothedRigidStateView>();
            Container.Decorate<IStateView<RigidState>>().With<KinematicFollowView<RigidState>>();
        }
    }
}
