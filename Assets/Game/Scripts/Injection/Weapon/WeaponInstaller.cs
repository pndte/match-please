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

            NetTablesInstaller.Install(Container);
            var netFactory = Container.Resolve<INetPropertyFactory>();
            var netTable = Container.Resolve<INetVariablesTable>();//TODO: костыль, фиксить
            
            OwnershipServicesInstaller.Install(Container, _runtimeSettings, netFactory);
            ControlledByServicesInstaller.Install(Container, _runtimeSettings, netFactory);

            BindConfigs();
            BindLifetime();

            BindAmmo(netFactory);
            BindWeaponRequests();
            BindCommonWeaponLogic();
            BindVfxRenderer();
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
                    Container.Bind<WeaponRequestsClientHandler>().AsSingle().NonLazy();
                    Container.Bind<ShootingWeapon.NetworkHandler>().AsSingle().NonLazy();
                    break;
                case PeerType.Server:
                    Container.Bind<WeaponRequestsServerHandler>().AsSingle().NonLazy();
                    break;
            }
        }

        private void BindWeaponRequests()
        {
            Container.BindInterfacesAndSelfTo<WeaponRequests>().FromMethod(ctx =>
            {
                var factory = ctx.Container.Resolve<INetPropertyFactory>();
                return new WeaponRequests(
                    factory.Signal<ShootRequestDto>(NetworkDelivery.Reliable, NetworkPermissions.Client),
                    factory.Signal<Unit>(NetworkDelivery.Reliable, NetworkPermissions.Client),
                    factory.Signal<ShootRequestDto>(NetworkDelivery.Reliable, NetworkPermissions.Server));
            }).AsSingle();
        }

        private void BindVfxRenderer()
        {
            Container.BindInterfacesTo<LineRendererVfxPlayer>().AsSingle().WithArguments(_trailPrefab);
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

        private void BindAmmo(INetPropertyFactory netFactory)
        {
            // Eager registration so VarId matches on client/server (lazy CreatePropertyFor registered Ammo after shoot signals on client).
            var ammoProperty = netFactory.Viewable(
                _shootingWeaponConfig.AmmoSettings.Max,
                NetworkDelivery.Reliable,
                NetworkPermissions.Server);

            Container.Bind<IViewableProperty<int>>()
                .FromInstance(ammoProperty)
                .AsSingle()
                .WhenInjectedInto<Ammo>();

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
