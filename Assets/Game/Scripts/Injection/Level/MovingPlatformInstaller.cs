using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Prediction.Bubble;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Variables;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.Injection.Network;
using Bw.Injection.Network.Variables;
using Bw.Injection.Ownership;
using Bw.UseCases.Level;
using Bw.UseCases.Level.Network;
using Bw.UseCases.Movement.Physics;
using Bw.UseCases.Movement.Physics.Abstractions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Zenject;

namespace Bw.Injection.Level
{
    public class MovingPlatformInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [Header("Network")]
        [SerializeField] private NetworkObject _networkObject;
        [SerializeField] private NetworkLifetimedBehaviour _networkLifetimedBehaviour;

        [Header("Platform")]
        [SerializeField] private BodyBox _box;
        [SerializeField] private MovingPlatformPath _path = new();

        public override void InstallBindings()
        {
            RequireTravel();
            RequireCarriage();
            OwnershipInstaller.Install(Container, _runtimeSettings);

            Container.Bind<NetworkObject>().FromInstance(_networkObject).AsSingle();
            Container.Bind<INetworkLifetimedObject>().FromInstance(_networkLifetimedBehaviour).AsSingle().NonLazy();
            Container.BindInstance(gameObject.Lifetime()).AsSingle();
            Container.Bind<BodyBox>().FromInstance(_box).AsSingle();
            BindPlatform();

            var netSchema = new NetEntriesSchemaBuilder();
            ReplicationInstaller<PlatformState>.Install(Container, _runtimeSettings, netSchema);
            NetTablesInstaller.Install(Container, _runtimeSettings, netSchema.Build());

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
        }

        private void BindPlatform()
        {
            Container.Bind<PhysicsBody>().FromMethod(CreatePhysicsBody).AsSingle();
            Container.BindInstance(new PlatformRail(_box.transform.position, _path.Travel)).AsSingle();
            Container.BindInstance(_path).AsSingle();
            Container.Bind(typeof(MovingPlatform), typeof(IMovingPlatform), typeof(IReadonlySimulation<PlatformState>), typeof(IRestorable<PlatformState>))
                .To<MovingPlatform>().AsSingle().WithArguments(Boxes)
                .WriterOnlyInto<IRestorable<PlatformState>>(typeof(RollbackPart<PlatformState>));
            Container.Bind<MovingPlatformRegistration>().AsSingle().NonLazy();
            Container.Bind<IStateView<PlatformState>>().To<PlatformStateView>().AsSingle();
        }

        private void BindServer()
        {
            Container.Bind<MovingPlatformSchedule>().AsSingle().NonLazy();
            LagCompensationInstaller<PlatformState, PlatformPoseView>.Install(Container, _runtimeSettings);
        }

        private void BindClient()
        {
            Container.Bind(typeof(IBubbleMembership), typeof(IPredictedBy)).To<BubbleMembership>().AsSingle().WithArguments(_box.transform);
            Container.Bind<PredictedPlatformSchedule>().AsSingle().NonLazy();
            Container.Bind<IStateDivergence<PlatformState>>().To<PlatformDivergence>().AsSingle();
            Container.Bind<RollbackPart<PlatformState>>().AsSingle();
            Container.Bind<RollbackPartRegistration<PlatformState>>().AsSingle().NonLazy();
            Container.Bind<PredictionInterpolation<PlatformState>>().AsSingle().NonLazy();
            BindClientVisuals();
        }

        private void BindClientVisuals()
        {
            Container.Decorate<IStateView<PlatformState>>().With<PlatformFollowView>();
            Container.Decorate<IStateView<PlatformState>>().With<PlatformTimelineBlendView>();
        }

        private BodyBox[] Boxes => _box.GetComponents<BodyBox>();

        private PhysicsBody CreatePhysicsBody(InjectContext context) =>
            context.Container.Resolve<IPhysicsSpace>().CreateBody(gameObject.Lifetime(), Boxes, PhysicsBody.BodyType.Kinematic);

        private void RequireTravel()
        {
            if (_path.Travel == Vector2.zero)
                throw new InvalidOperationException($"Moving platform '{name}' has no travel: its state is a share of the travel, and a zero travel has none.");
        }

        private void RequireCarriage()
        {
            if (_box.transform == transform)
                throw new InvalidOperationException(
                    $"Moving platform '{name}' carries its box on the root: NGO spawns a late-joining client's copy where the root is, so the root must stay at the rail's start and a child must move.");

            if (GetComponentsInChildren<BodyBox>(true).Length != Boxes.Length)
                throw new InvalidOperationException(
                    $"Moving platform '{name}' has a box off its carriage '{_box.name}': a floor, its walls and its roof are all boxes on the carriage, which is the one body that moves.");
        }
    }
}
