using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
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

        [SerializeField] private BodyBox _box;
        [SerializeField] private MovingPlatformPath _path = new();

        public override void InstallBindings()
        {
            Container.BindInstance(gameObject.Lifetime()).AsSingle();
            Container.Bind<PhysicsBody>().FromMethod(CreatePhysicsBody).AsSingle();
            Container.Bind<SweptBody>().AsSingle();
            Container.Bind<SweptBodyRegistration>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MovingPlatform>().AsSingle().WithArguments(_box, _path);
            Container.Bind<MovingPlatformRegistration>().AsSingle().NonLazy();
            Container.Bind<MovingPlatformSchedule>().AsSingle().NonLazy();

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<MovingPlatformRewinder>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    BindVisuals();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindVisuals() =>
            Container.Bind<MovingPlatformInterpolation>().AsSingle().NonLazy();

        private PhysicsBody CreatePhysicsBody(InjectContext context) =>
            context.Container.Resolve<IPhysicsSpace>().CreateBody(gameObject.Lifetime(), _box, PhysicsBody.BodyType.Kinematic);
    }
}
