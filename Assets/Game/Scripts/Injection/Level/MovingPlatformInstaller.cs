using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Level;
using Bw.UseCases.Level.Network;
using Bw.UseCases.Movement.Physics;
using UnityEngine;
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

        private void RequireMovingBody()
        {
            if (_body.bodyType != RigidbodyType2D.Kinematic)
                throw new InvalidOperationException($"Moving platform {name} needs a kinematic body: the schedule places it every tick.");

            if (_collider.attachedRigidbody != _body || _collider.transform != _body.transform)
                throw new InvalidOperationException($"Moving platform {name}: its collider must sit on the moving body itself.");
        }
    }
}
