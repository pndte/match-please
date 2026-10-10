using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Network;
using Bw.Injection.Network.Variables;
using Bw.UseCases.Character;
using Bw.UseCases.Shooting;
using Bw.UseCases.Shooting.View;
using Bw.UseCases.Shooting.View.Audio;
using Bw.UseCases.Shooting.View.Crosshair;
using Bw.UseCases.Shooting.View.Recoil;
using Bw.UseCases.Shooting.View.Rig;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Network;
using Bw.UseCases.Shooting.Weapon.Network.Requests;
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

        public override void InstallBindings()
        {
            RequireTurnSpeed();
            BindConfigs();
            BindGun();

            var netSchema = new NetEntriesSchemaBuilder();
            HeldWeaponInstaller.Install(Container, _runtimeSettings, netSchema, Parts());
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

        private HeldWeaponParts Parts() =>
            new(
                transform,
                _visual,
                _networkObject,
                _weaponHold,
                _networkTransform,
                _body,
                new WeaponState(0f, _shootingWeaponConfig.AmmoSettings.OnSpawnValue, 0, 0, 0),
                KeyCode.Alpha2); //TODO: new input system

        private void BindGun()
        {
            Container.BindInterfacesTo<WeaponSimulator>().AsSingle();
            Container.BindInterfacesTo<WeaponMuzzle>().AsSingle().WithArguments(_muzzleTransform);
            Container.BindInterfacesTo<ShotTracer>().AsSingle().WithArguments(transform);
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
            Container.Bind<WeaponSpreadCursor>().AsSingle().WithArguments(transform).NonLazy();
            BindClientVisuals();
            BindClientSounds();
        }

        private void BindClientVisuals()
        {
            Container.Bind<IStateView<WeaponState>>()
                .FromMethod(ctx => new CompositeStateView<WeaponState>(
                    ctx.Container.Instantiate<WeaponStateView>(new object[] { transform }),
                    ctx.Container.Resolve<SeenShots>(),
                    ctx.Container.Instantiate<WeaponReloadCursor>(),
                    ctx.Container.Instantiate<WeaponReloadSoundsView>(new object[] { transform }),
                    ctx.Container.Instantiate<WeaponRigView>(new object[] { _rig })))
                .AsSingle();
            Container.Bind<WeaponShotEffectsView>().AsSingle().NonLazy();
        }

        private void BindClientSounds()
        {
            Container.Bind<WeaponSoundsConfig>().FromMethod(ctx => ctx.Container.Resolve<WeaponSoundsCatalog>().For(_shootingWeaponConfig)).AsSingle();
            Container.BindInterfacesTo<ShotSfxPlayer>().AsSingle();
        }

        private void BindConfigs()
        {
            Container.BindInstance(_raycastShootConfig).AsSingle();
            Container.BindInstance(_vfxConfig).AsSingle();
            Container.BindInstance(_shootingWeaponConfig).AsSingle();
            Container.Bind<IHitConfig>().FromInstance(_shootingWeaponConfig).AsSingle();
            Container.BindInstance(_rotationConfig).AsSingle();
            Container.BindInstance(_cameraKickConfig).AsSingle();
            Container.BindInstance(_recoilConfig).AsSingle();
        }
    }
}
