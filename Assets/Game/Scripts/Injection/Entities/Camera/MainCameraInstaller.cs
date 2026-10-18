using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Physics;
using Bw.UseCases.Audio.View.Playback;
using Bw.UseCases.Camera.View.Follow;
using Bw.UseCases.Camera.View.Shake;
using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Character.View.HealthBar;
using Bw.UseCases.FogOfWar.View;
using Bw.UseCases.FogOfWar.View.Sight;
using Bw.UseCases.Props.View;
using Bw.UseCases.Props.View.Abstractions;
using Bw.UseCases.Props.View.Aim;
using Bw.UseCases.Shooting.View.Crosshair;
using Bw.UseCases.Shooting.View.Crosshair.Abstractions;
using Bw.UseCases.Vfx.View.Effects;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Zenject;

namespace Bw.Injection.Entities.Camera
{
    public class MainCameraInstaller : MonoInstaller
    {
        [Inject] IRuntimeSettings _runtimeSettings;

        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] private CameraFollowConfig _followConfig;

        [Header("Fog of war")]
        [SerializeField] private bool _fogOfWar = true;
        [SerializeField] private FogOfWarConfig _fogOfWarConfig;

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
            Container.BindInterfacesTo<EffectPlayer>().AsSingle().WithArguments(_camera.gameObject.Lifetime());
            BindHealthBars();
            BindThrowArc();

            if (_fogOfWar)
                BindFogOfWar();
        }

        private void BindHealthBars() =>
            Container.Bind<IHealthBars>().To<HealthBars>().FromComponentOn(_camera.gameObject).AsSingle();

        private void BindThrowArc() =>
            Container.Bind<IThrowArcPreview>()
                .FromMethod(_ => CreateThrowArcPreview(Container.Resolve<PropVfxConfig>()))
                .AsSingle();

        private ThrowArcPreview CreateThrowArcPreview(PropVfxConfig config) =>
            new(_camera.gameObject.Lifetime(), Container.InstantiatePrefabForComponent<ParticleSystem>(config.ArcDots), config);

        private void BindFogOfWar()
        {
            RequireFogOfWarConfig();
            Container.Bind<FogOfWarView>()
                .FromMethod(_ => FogOfWarView.Create(_camera.gameObject.Lifetime(), _camera, Container.Resolve<ICameraTargets>(), _fogOfWarConfig, CreateSight()))
                .AsSingle()
                .NonLazy();
        }

        private LineOfSight CreateSight()
        {
            var seeThrough = PhysicsMask.None;
            seeThrough.SetBit(PhysicsCategories.OneWay);
            return new LineOfSight(Container.Resolve<IPhysicsSpace>().World, seeThrough, _fogOfWarConfig);
        }

        private void RequireFogOfWarConfig()
        {
            if (_fogOfWarConfig == null)
                throw new InvalidOperationException("The fog of war is on, but the main camera installer has no fog of war config.");

            if (_fogOfWarConfig.Blockers.value == 0)
                throw new InvalidOperationException($"Fog of war config '{_fogOfWarConfig.name}' has no blocker layers: nothing would hide anything.");

            if (_fogOfWarConfig.MaskShader == null || _fogOfWarConfig.HidingShader == null || _fogOfWarConfig.FogShader == null)
                throw new InvalidOperationException($"Fog of war config '{_fogOfWarConfig.name}' misses one of its shaders.");

            if (!_camera.orthographic)
                throw new InvalidOperationException("The fog of war traces an orthographic view, but the main camera is perspective.");
        }
    }
}
