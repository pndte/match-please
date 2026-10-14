using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Network;
using Bw.Injection.Network.Variables;
using Bw.UseCases.Mortality;
using Bw.UseCases.Shooting;
using Bw.UseCases.Shooting.View;
using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Shooting.View.Adapters;
using Bw.UseCases.Shooting.View.Adapters.Audio;
using Bw.UseCases.Shooting.View.Audio;
using Bw.UseCases.Shooting.View.Crosshair;
using Bw.UseCases.Shooting.View.Highlight;
using Bw.UseCases.Shooting.View.Recoil;
using Bw.UseCases.Shooting.View.Rig;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Network;
using Bw.UseCases.Shooting.Weapon.Network.Requests;
using Bw.UseCases.Shooting.Weapon.Pickup;
using JetBrains.Core;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
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

        [Header("Network")]
        [SerializeField] private NetworkObject _networkObject;
        [SerializeField] private NetworkWeaponHold _weaponHold;
        [SerializeField] private NetworkTransform _networkTransform;

        [Header("Physics")] [SerializeField] private Rigidbody2D _body;
        [SerializeField] private Collider2D _collider;

        public override void InstallBindings()
        {
            RequireTurnSpeed();
            BindConfigs();
            BindGun();

            var netSchema = new NetEntriesSchemaBuilder();
            HeldWeaponInstaller<GunState>.Install(Container, _runtimeSettings, netSchema, Parts());
            BindDropRequest(netSchema);
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

        private HeldWeaponParts<GunState> Parts() =>
            new(
                transform,
                _visual,
                _networkObject,
                _weaponHold,
                _networkTransform,
                _body,
                new GunState(0f, _shootingWeaponConfig.AmmoSettings.OnSpawnValue, 0, 0, 0),
                WeaponKind.Gun);

        private void BindGun()
        {
            Container.BindInterfacesTo<GunSimulator>().AsSingle();
            Container.BindInterfacesTo<WeaponMuzzle>().AsSingle().WithArguments(_muzzleTransform);
            Container.BindInterfacesTo<ShotTracer>().AsSingle().WithArguments(transform);
            Container.BindInterfacesTo<FixedShotPower>().AsSingle();
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
            Container.Bind<UnheldWeaponDespawner>().AsSingle().NonLazy();
            Container.InstantiateComponent<WeaponHolder>(gameObject);
        }

        private void BindClient()
        {
            Container.Bind<WeaponDropClientHandler>().AsSingle().NonLazy();
            Container.BindInterfacesTo<Spread>().AsSingle();
            Container.BindInterfacesTo<AimDeflection>().AsSingle();
            Container.Decorate<IInputSampler<WeaponInput>>().With<SpreadAimSampler>();
            Container.Bind<SpreadKick>().AsSingle().NonLazy();
            Container.BindInterfacesTo<FloorWeapon>().AsSingle().WithArguments(_collider);
            Container.Bind<FloorWeaponRegistration>().AsSingle().NonLazy();
            BindClientVisuals();
            BindClientSounds();
        }

        private void BindClientVisuals()
        {
            Container.Bind(typeof(IGunViewModel), typeof(GunViewModel)).To<GunViewModel>().AsSingle();
            Container.Bind<IStateView<GunState>>()
                .FromMethod(ctx => new CompositeStateView<GunState>(
                    ctx.Container.Instantiate<WeaponStateView<GunState>>(new object[] { transform }),
                    ctx.Container.Resolve<SeenShotsViewModel<GunState>>(),
                    ctx.Container.Resolve<GunViewModel>()))
                .AsSingle();
            Container.Bind<WeaponReloadCursor>().AsSingle().NonLazy();
            Container.Bind<WeaponRoundsCursor>().AsSingle().NonLazy();
            Container.Bind<WeaponRigView>().AsSingle().WithArguments(_rig).NonLazy();
            Container.Bind<WeaponShotEffectsView>().AsSingle().NonLazy();
            Container.Bind<IMuzzleFlash>().FromComponentInNewPrefab(_vfxConfig.MuzzleFlash).UnderTransform(_flashPoint).AsSingle();
            Container.Bind<MuzzleEffectsView>().AsSingle().WithArguments(_ejector).NonLazy();
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
