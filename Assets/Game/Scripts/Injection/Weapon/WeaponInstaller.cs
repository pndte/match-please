using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Repository;
using Bw.Entities.Network.Variables;
using Bw.Injection.ControlledBy;
using Bw.Injection.Network;
using Bw.Injection.Network.Variables;
using Bw.Injection.Ownership;
using Bw.UseCases.Shooting;
using Bw.UseCases.Shooting.View;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Network;
using Bw.UseCases.Shooting.Weapon.Network.Requests;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Weapon
{
    public class WeaponInstaller : MonoInstaller // todo: decompose
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [Header("Graphics")] [SerializeField] private LineRenderer _trailPrefab;
        [SerializeField] private SpriteRenderer _sprite;

        [Header("WeaponMuzzle")] [SerializeField] private Transform _muzzleTransform;

        [Header("Configs")] [SerializeField] private RaycastShootConfig _raycastShootConfig;
        [SerializeField] private ShootingWeaponConfig _shootingWeaponConfig;
        [SerializeField] private LineRendererVfxConfig _vfxConfig;

        [Header("Network")]
        [SerializeField] private NetworkObject _networkObject;
        [SerializeField] private WeaponRotation _weaponRotation;

        [Header("Loop")] [SerializeField] private ShootingWeaponLoopRunner _weaponLoopRunner;

        public override void InstallBindings()
        {
            OwnershipInstaller.Install(Container, _runtimeSettings);
            ControlledByInstaller.Install(Container, _runtimeSettings);

            Container.Bind<NetworkObject>().FromInstance(_networkObject).AsSingle();
            Container.Bind<INetworkLifetimedObject>().FromInstance(_weaponRotation).AsSingle();
            BindLifetime();

            var netSchema = new NetEntriesSchemaBuilder();
            OwnershipServicesInstaller.Install(Container, _runtimeSettings, netSchema);
            ControlledByServicesInstaller.Install(Container, _runtimeSettings, netSchema);

            BindConfigs();

            BindAmmo(netSchema);
            BindWeaponRequests(netSchema);
            NetTablesInstaller.Install(Container, netSchema.Build());

            BindCommonWeaponLogic();
            BindVfxRenderer();
            BindSprite();
            BindRequestHandlers();

            BindSpecialWeaponLogic();

            BindLoop();

            BindWeaponHolder();

            Container.Bind<WeaponRotation>().FromInstance(_weaponRotation).AsSingle().NonLazy();
        }

        private void BindWeaponHolder()
        {
            if (_runtimeSettings.CurrentPeerType == PeerType.Server)
                Container.InstantiateComponent<WeaponHolder>(gameObject);
        }

        private void BindLoop()
        {
            Container.Bind<ShootingWeaponLoopRunner>().FromInstance(_weaponLoopRunner).AsSingle().NonLazy();
        }

        private void BindSpecialWeaponLogic()
        {
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<RaycastShooter>().AsSingle().NonLazy();
                    Container.Bind<WeaponAmmoManager>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind<BulletTrailRenderer>().AsSingle().NonLazy();
                    break;
            }
        }

        private void BindRequestHandlers()
        {
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Client:
                    Container.BindInterfacesTo<RequestIdsRepository>().AsSingle();
                    Container.Bind<PendingReloadLifetimes>().AsSingle();
                    Container.Bind<WeaponRequestsClientHandler>().AsSingle().NonLazy();
                    Container.Bind<ShootingWeapon.NetworkHandler>().AsSingle().NonLazy();
                    Container.Bind<InterruptableReloader.NetworkHandler>().AsSingle().NonLazy();
                    break;
                case PeerType.Server:
                    Container.Bind<WeaponRequestsServerHandler>().AsSingle().NonLazy();
                    break;
            }
        }

        private void BindWeaponRequests(INetEntriesSchemaBuilder netSchema)
        {
            var mouseShootRequestDeclaration = netSchema.DeclareSignal<ShootRequestDto>(NetworkDelivery.Reliable, NetworkPermissions.Client);
            var reloadRequestDeclaration = netSchema.DeclareSignal<ReloadRequestDto>(NetworkDelivery.Reliable, NetworkPermissions.Client);
            var shootReceivedDeclaration = netSchema.DeclareSignal<ShootRequestResultDto>(NetworkDelivery.Reliable, NetworkPermissions.Server);
            var reloadReceivedDeclaration = netSchema.DeclareSignal<ReloadRequestResultDto>(NetworkDelivery.Reliable, NetworkPermissions.Server);

            Container.BindInterfacesAndSelfTo<WeaponSignals>().FromMethod(ctx =>
            {
                var entries = ctx.Container.Resolve<INetEntries>();
                return new WeaponSignals(
                    entries.Get(mouseShootRequestDeclaration),
                    entries.Get(reloadRequestDeclaration),
                    entries.Get(shootReceivedDeclaration),
                    entries.Get(reloadReceivedDeclaration));
            }).AsSingle();
        }

        private void BindVfxRenderer()
        {
            Container.BindInterfacesTo<LineRendererVfxPlayer>().AsSingle().WithArguments(_trailPrefab);
        }

        private void BindSprite()
        {
            Container.Bind<WeaponSpriteFlip>().AsSingle().WithArguments(transform, _sprite).NonLazy();
        }

        private void BindConfigs()
        {
            Container.BindInstance(_raycastShootConfig).AsSingle();
            Container.BindInstance(_vfxConfig).AsSingle();
            Container.BindInstance(_shootingWeaponConfig).AsSingle();
            Container.BindInstance(_shootingWeaponConfig.AmmoSettings).AsSingle();
        }

        private void BindCommonWeaponLogic()
        {
            Container.Bind<IViewableProperty<ReloadState>>()
                .FromInstance(new ViewableProperty<ReloadState>(ReloadState.Complete))
                .WhenInjectedInto<InterruptableReloader>();
            Container.BindInterfacesAndSelfTo<InterruptableReloader>().AsSingle();
            Container.BindInterfacesAndSelfTo<WeaponMuzzle>().AsSingle().WithArguments(_muzzleTransform);
            Container.BindInterfacesAndSelfTo<ShootingWeapon>().AsSingle();
        }

        private void BindAmmo(INetEntriesSchemaBuilder netSchema)
        {
            var ammoDeclaration = netSchema.DeclareProperty(
                _shootingWeaponConfig.AmmoSettings.OnSpawnValue,
                NetworkDelivery.Reliable,
                NetworkPermissions.Server);
            Container.BindNetPropertyFor<int, Ammo>(ammoDeclaration);

            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
            {
                Container.Bind<IReadonlyAmmo>().To<Ammo>().AsSingle();
            }
            else if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.Bind(typeof(IAmmo), typeof(IReadonlyAmmo)).To<Ammo>().AsSingle();
            }
        }

        private void BindLifetime()
        {
            var gameObjectLifetime = gameObject.Lifetime();
            Container.BindInstance(gameObjectLifetime).AsSingle();
        }
    }
}
