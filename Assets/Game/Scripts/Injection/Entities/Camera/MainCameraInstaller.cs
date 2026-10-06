using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Pool;
using Bw.Entities.Pool.GameObjects;
using Bw.Injection.Pool;
using Bw.UseCases.Audio.View.Playback;
using Bw.UseCases.Camera.View.Follow;
using Bw.UseCases.Camera.View.Shake;
using Bw.UseCases.Shooting.View.Crosshair;
using Bw.UseCases.Shooting.View.Crosshair.Abstractions;
using Bw.UseCases.Shooting.View.Impact;
using Bw.UseCases.Shooting.View.Impact.Abstractions;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Entities.Camera
{
    public class MainCameraInstaller : MonoInstaller
    {
        [Inject] IRuntimeSettings _runtimeSettings;

        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] private CameraFollowConfig _followConfig;

        public override void InstallBindings()
        {
            if (_runtimeSettings.CurrentPeerType != PeerType.Client)
                return;

            Container.BindInterfacesTo<PlayerCamera>().AsSingle().WithArguments(_camera).NonLazy();
            Container.Bind<ICameraShake>().To<CameraShake>().FromComponentOn(_camera.gameObject).AsSingle();
            Container.Bind<IAimCursor>().To<AimCursor>().FromComponentOn(_camera.gameObject).AsSingle();
            Container.BindInterfacesTo<CameraTargets>().AsSingle();
            Container.Bind<CameraFollow>().AsSingle().WithArguments(_camera.gameObject.Lifetime(), _camera, _followConfig).NonLazy();
            Container.BindInterfacesTo<SoundPlayer>().AsSingle().WithArguments(_camera.gameObject.Lifetime());
            BindImpactEffects();
        }

        private void BindImpactEffects()
        {
            Container.BindInterfacesTo<LimitedPool<IImpactEffect>>()
                .FromMethod(context =>
                {
                    var config = context.Container.Resolve<ShotImpactConfig>();
                    return PrefabPools.Create<IImpactEffect, ParticleImpactEffect>(_camera.gameObject.Lifetime(), config.Prefab, config.Pool.Settings());
                })
                .AsSingle();
            Container.BindInterfacesTo<PoolsPrewarm>().AsSingle();
        }
    }
}