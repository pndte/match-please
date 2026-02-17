using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.UseCases;
using Bw.UseCases.Shooting;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Network;
using Bw.UseCases.Shooting.Weapon.Triggers;
using Bw.UseCases.Shooting.Weapon.Triggers.Network;
using Setup;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Weapon
{
    public class WeaponInstaller : MonoInstaller // todo: decompose
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [Header("Ownership")] [SerializeField] private GameObjectMetaData _gameObjectMetaData;

        [Header("Graphics")] [SerializeField] private LineRenderer _trailPrefab;

        [Header("WeaponMuzzle")] [SerializeField] private Transform _muzzleTransform;

        [Header("Triggers")] [SerializeField] private ShootingWeaponTriggers _shootingWeaponTriggers;

        [Header("Ammo")] [SerializeField] private NetworkAmmo _networkAmmo;

        [Header("Configs")] [SerializeField] private RaycastShootConfig _raycastShootConfig;
        [SerializeField] private ShootingWeaponConfig _shootingWeaponConfig;
        [SerializeField] private LineRendererVfxConfig _vfxConfig;

        [Header("Network")] [SerializeField] private NetworkLifetimedBehaviour _networkLifetimedBehaviour;
        [Header("Loop")] [SerializeField] private ShootingWeaponLoopRunner _weaponLoopRunner;

        public override void InstallBindings()
        {
            BindOwnership();
            BindConfigs();
            BindLifetime();
            BindNetwork();

            BindAmmo();
            BindCommonWeaponLogic();
            BindVfxRenderer();
            BindKeyboardTriggers();

            BindSpecialWeaponLogic();

            BindLoop();

            BindWeaponHolder();
        }

        private void BindWeaponHolder()
        {
            if (_runtimeSettings.CurrentPeerType == PeerType.Server)
                Container.InstantiateComponent<WeaponHolder>(gameObject);
        }

        private void BindOwnership()
        {
            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
                Container.Bind(typeof(IReadonlyOwnership), typeof(IReadonlyControlledBy)).FromInstance(_gameObjectMetaData).AsSingle().NonLazy();
            else
                Container.BindInterfacesTo<GameObjectMetaData>().FromInstance(_gameObjectMetaData).AsSingle()
                    .NonLazy();
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
                    Container.Bind<ShootingWeaponServerHandler>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind<BulletTrailRenderer>().AsSingle().NonLazy();
                    break;
            }
        }

        private void BindKeyboardTriggers()
        {
            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
                Container.BindInterfacesAndSelfTo<KeyboardWeaponTriggersConnector>().AsSingle()
                    .NonLazy(); //TODO: должно устанавливаться только на клиенте. Но тогда будет ошибка в лупе, тк объект не найдёт
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
            Container.Bind(typeof(IReloadTrigger), typeof(IMouseShootTrigger))
                .To<ShootingWeaponTriggers>().FromInstance(_shootingWeaponTriggers).AsSingle()
                .NonLazy(); // TODO: owner Lifetime
            Container.BindInterfacesAndSelfTo<WeaponMuzzle>().AsSingle().WithArguments(_muzzleTransform);
            Container.BindInterfacesAndSelfTo<ShootingWeapon>().AsSingle();
        }

        private void BindAmmo()
        {
            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
            {
                Container.Bind<IReadonlyAmmo>().To<NetworkAmmo>().FromInstance(_networkAmmo).AsSingle();
            }
            else if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.Bind(typeof(IAmmo), typeof(IReadonlyAmmo)).To<NetworkAmmo>().FromInstance(_networkAmmo)
                    .AsSingle();
            }
        }

        private void BindNetwork()
        {
            Container.BindInterfacesTo<NetworkLifetimedBehaviour>().FromInstance(_networkLifetimedBehaviour).AsSingle();
        }

        private void BindLifetime()
        {
            var gameObjectLifetime = gameObject.Lifetime();
            Container.BindInstance(gameObjectLifetime).AsSingle();
        }
    }
}