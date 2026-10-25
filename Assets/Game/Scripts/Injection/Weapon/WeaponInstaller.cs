using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using Bw.Entities.Physics;
using Bw.Entities.Simulation;
using Bw.Injection.Movement;
using Bw.Injection.Network;
using Bw.Injection.Network.Variables;
using Bw.UseCases.Combat;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Shooting.Network;
using Bw.UseCases.Combat.View;
using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.View.Adapters;
using Bw.UseCases.Combat.View.Adapters.Audio;
using Bw.UseCases.Combat.View.Audio;
using Bw.UseCases.Combat.View.Crosshair;
using Bw.UseCases.Combat.View.Highlight;
using Bw.UseCases.Combat.View.Recoil;
using Bw.UseCases.Combat.View.Rig;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Combat.Weapon.Pickup;
using Bw.UseCases.Mortality;
using Bw.UseCases.Movement;
using JetBrains.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Bw.Injection.Weapon
{
    public class WeaponInstaller : MonoInstaller
    {
        public const string VisualField = nameof(_visual);
        public const string MuzzleField = nameof(_muzzleTransform);
        public const string ShootingConfigField = nameof(_shootingWeaponConfig);
        public const string RotationConfigField = nameof(_rotationConfig);
        public const string RecoilConfigField = nameof(_recoilConfig);

        [Inject] private IRuntimeSettings _runtimeSettings;

        [Header("Graphics")] [SerializeField] private Transform _visual;
        [SerializeField] private Animator _rig;
        [SerializeField] private Transform _flashPoint;
        [SerializeField] private Transform _ejector;

        [Header("WeaponMuzzle")] [SerializeField] private Transform _muzzleTransform;

        [Header("Configs")] [SerializeField] private RaycastShootConfig _raycastShootConfig;
        [SerializeField] private ShootingWeaponConfig _shootingWeaponConfig;
        [SerializeField] private WeaponRotationConfig _rotationConfig;
        [SerializeField] private ShotVfxConfig _vfxConfig;
        [SerializeField] private WeaponCameraKickConfig _cameraKickConfig;
        [SerializeField] private WeaponRecoilConfig _recoilConfig;
        [SerializeField] private FlashlightConfig _flashlightConfig;

        [Header("Network")]
        [SerializeField] private NetworkObject _networkObject;
        [FormerlySerializedAs("_weaponHold")] [SerializeField] private NetworkLifetimedBehaviour _networkLifetimedBehaviour;

        [Header("Physics")] [SerializeField] private BodyBox _box;
        [SerializeField] private DynamicBodyConfig _bodyConfig;
        [SerializeField] private WeaponReleaseConfig _releaseConfig;

        public override void InstallBindings()
        {
            RequireTurnSpeed();
            RequireFlashlightBeam();
            RequireReleaseConfig();
            BindConfigs();
            BindGun();

            var netSchema = new NetEntriesSchemaBuilder();
            HeldWeaponInstaller<GunState>.Install(Container, _runtimeSettings, netSchema, Parts());
            BindDropRequest(netSchema);
            FlashlightInstaller.Install(Container, _runtimeSettings, netSchema);
            BindBody(netSchema);
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

        private void RequireTurnSpeed()
        {
            if (_shootingWeaponConfig.TurnSpeed <= 0f)
                throw new InvalidOperationException(
                    $"Weapon config '{_shootingWeaponConfig.name}' has no turn speed: the gun would never turn to the cursor.");
        }

        private void RequireFlashlightBeam()
        {
            if (_flashlightConfig == null || _flashlightConfig.Beam == null)
                throw new InvalidOperationException($"Weapon '{name}' has no flashlight beam: its WeaponInstaller needs a FlashlightConfig with a beam prefab.");
        }

        private void RequireReleaseConfig()
        {
            if (_releaseConfig == null)
                throw new InvalidOperationException($"Weapon '{name}' has no release config: its WeaponInstaller needs a WeaponReleaseConfig to let the gun go.");
        }

        private HeldWeaponParts<GunState> Parts() =>
            new(
                transform,
                _visual,
                _networkObject,
                _networkLifetimedBehaviour,
                new GunState(0f, _shootingWeaponConfig.AmmoSettings.OnSpawnValue, 0, 0, 0),
                WeaponKind.Gun);

        private void BindGun()
        {
            WeaponSimulatorInstaller<GunState, GunSimulator>.Install(Container);
            Container.Bind<WeaponMuzzle>().AsSingle().WithArguments(_muzzleTransform);
            Container.BindInterfacesTo<ShotTracer>().AsSingle();
            Container.BindInterfacesTo<FixedShotPower>().AsSingle();
        }

        private void BindBody(INetEntriesSchemaBuilder netSchema)
        {
            Container.Bind<BodyBox>().FromInstance(_box).AsSingle();
            DynamicBodyInstaller.Install(Container, _runtimeSettings, netSchema, _bodyConfig);
            Container.Decorate<IStateView<RigidState>>().With<UnheldStateView<RigidState>>();
            Container.Bind<HeldWeaponBody>().AsSingle().NonLazy();
        }

        private void BindDropRequest(INetEntriesSchemaBuilder netSchema)
        {
            var drop = netSchema.DeclareRequest<Unit>(NetworkDelivery.Reliable);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindRequestReceiver(drop).WhenInjectedInto<WeaponDropServerHandler>();
                    break;
                case PeerType.Client:
                    Container.BindRequestSender(drop).WhenInjectedInto<WeaponDropClientHandler>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindServer()
        {
            Container.Bind<WeaponDropServerHandler>().AsSingle().NonLazy();
            Container.Bind<HeldWeaponPose>().AsSingle();
            Container.Bind<WeaponReleaseSpots>().AsSingle().WithArguments(_releaseConfig);
            Container.Bind<WeaponRelease>().AsSingle().NonLazy();
            Container.Bind<UnheldWeaponDespawner>().AsSingle().NonLazy();
            Container.InstantiateComponent<WeaponHolder>(gameObject);
        }

        private void BindClient()
        {
            Container.Bind<WeaponDropClientHandler>().AsSingle().NonLazy();
            Container.BindInterfacesTo<Spread>().AsSingle();
            Container.BindInterfacesTo<AimDeflection>().AsSingle();
            Container.Bind<SpreadKick>().AsSingle();
            Container.BindInterfacesTo<FloorWeapon>().AsSingle().WithArguments(_box);
            Container.Bind<FloorWeaponRegistration>().AsSingle().NonLazy();
            BindClientVisuals();
            BindClientSounds();
        }

        private void BindClientVisuals()
        {
            Container.Bind(typeof(IGunViewModel), typeof(GunViewModel)).To<GunViewModel>().AsSingle();
            Container.Bind<IStateView<GunState>>()
                .FromMethod(ctx => new CompositeStateView<GunState>(
                    ctx.Container.Instantiate<SmoothedAimView<GunState>>(new object[] { ctx.Container.Resolve<WeaponPoseViewModel<GunState>>() }),
                    ctx.Container.Resolve<SeenShotsViewModel<GunState>>(),
                    ctx.Container.Resolve<GunViewModel>(),
                    ctx.Container.Resolve<SpreadKick>()))
                .AsSingle();
            Container.Bind<WeaponReloadCursor>().AsSingle().NonLazy();
            Container.Bind<WeaponRoundsCursor>().AsSingle().NonLazy();
            Container.Bind<WeaponRigView>().AsSingle().WithArguments(_rig).NonLazy();
            Container.Bind<WeaponShotEffectsView>().AsSingle().NonLazy();
            Container.Bind<IMuzzleFlash>().FromComponentInNewPrefab(_vfxConfig.MuzzleFlash).UnderTransform(_flashPoint).AsSingle();
            Container.Bind<MuzzleEffectsView>().AsSingle().WithArguments(_ejector).NonLazy();
            Container.BindInterfacesTo<FlashlightViewModel>().AsSingle();
            Container.Bind<IFlashlightBeam>().FromComponentInNewPrefab(_flashlightConfig.Beam).UnderTransform(_flashPoint).AsSingle();
            Container.Bind<FlashlightView>().AsSingle().NonLazy();
            Container.BindInterfacesTo<FloorWeaponHighlightViewModel>().AsSingle();
            Container.Bind<WeaponHighlightView>().AsSingle().WithArguments(_visual, _visual.GetComponentsInChildren<SpriteRenderer>(true)).NonLazy();
        }

        private void BindClientSounds()
        {
            Container.Bind<GunSounds>().FromMethod(ctx => ctx.Container.Resolve<WeaponSoundsCatalog>().For(_shootingWeaponConfig)).AsSingle();
            Container.BindInterfacesTo<ShotSfxPlayer>().AsSingle().WithArguments(_muzzleTransform);
            Container.Bind<WeaponReloadSoundsView>().AsSingle().WithArguments(transform).NonLazy();
        }

        private void BindConfigs()
        {
            Container.BindInstance(_raycastShootConfig).AsSingle();
            Container.BindInstance(_vfxConfig).AsSingle();
            Container.BindInstance(_shootingWeaponConfig).AsSingle();
            Container.BindInstance(_shootingWeaponConfig.Pellets).AsSingle();
            Container.Bind<IHitConfig>().FromInstance(_shootingWeaponConfig).AsSingle();
            Container.BindInstance(_rotationConfig).AsSingle();
            Container.BindInstance(_cameraKickConfig).AsSingle();
            Container.BindInstance(_recoilConfig).AsSingle();
        }
    }
}
