using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Level;
using Bw.UseCases.Level.Network;
using Bw.UseCases.Movement.Physics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Zenject;

namespace Bw.Injection.Level
{
    public class MovingPlatformInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [SerializeField] private Rigidbody2D _body;
        [SerializeField] private BoxCollider2D _collider;
        [SerializeField] private MovingPlatformPath _path = new();

        public override void InstallBindings()
        {
            RequireMovingBody();

            Container.BindInstance(gameObject.Lifetime()).AsSingle();
            Container.Bind<PhysicsBody>().FromMethod(CreatePhysicsBody).AsSingle();
            Container.Bind<SweptBody>().AsSingle();
            Container.Bind<SweptBodyRegistration>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MovingPlatform>().AsSingle().WithArguments(_body, _collider, _path);
            Container.Bind<MovingPlatformRegistration>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MovingPlatformSchedule>().AsSingle().NonLazy();
            Container.Bind<ScheduledRegistration>().AsSingle().NonLazy();

            if (_runtimeSettings.CurrentPeerType != PeerType.Client)
                return;

            BindVisuals();
        }

        private void BindVisuals() =>
            Container.Bind<MovingPlatformInterpolation>().AsSingle().NonLazy();

        private PhysicsBody CreatePhysicsBody(InjectContext context) =>
            context.Container.Resolve<IPhysicsSpace>().CreateBody(gameObject.Lifetime(), _collider, PhysicsBody.BodyType.Kinematic);

        private void RequireMovingBody()
        {
            if (_body.bodyType != RigidbodyType2D.Kinematic)
                throw new InvalidOperationException($"Moving platform {name} needs a kinematic body: the schedule places it every tick.");

            if (_collider.attachedRigidbody != _body || _collider.transform != _body.transform)
                throw new InvalidOperationException($"Moving platform {name}: its collider must sit on the moving body itself.");
        }
    }
}
