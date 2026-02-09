using Bw.UseCases.Shooting;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Network;
using Bw.UseCases.Shooting.Weapon.Triggers;
using Setup;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Weapon
{
    public class WeaponInstaller : MonoInstaller // todo: decompose
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [SerializeField] private LineRenderer _trailPrefab;
        [SerializeField] private Transform _muzzleTransform;

        [Header("Triggers")] [SerializeField] private ShootingWeaponTriggers _shootingWeaponTriggers;

        [Header("Ammo")] [SerializeField] private NetworkAmmo _networkAmmo;

        [Header("Configs")] [SerializeField] private RaycastShootConfig _raycastShootConfig;
        [SerializeField] private ShootingWeaponConfig _shootingWeaponConfig;
        [SerializeField] private LineRendererVfxConfig _vfxConfig;


        public override void InstallBindings()
        {
            Container.BindInstance(_raycastShootConfig).AsSingle();
            Container.BindInstance(_vfxConfig).AsSingle();
            Container.BindInstance(_shootingWeaponConfig).AsSingle();
            Container.BindInstance(_shootingWeaponConfig).AsSingle();

            Container.BindInterfacesTo<ShootingWeaponTriggers>().AsSingle().NonLazy();

            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
            {
                Container.Bind<IReadonlyAmmo>().To<NetworkAmmo>().FromInstance(_networkAmmo).AsSingle();
            }
            else if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.Bind<IAmmo>().To<NetworkAmmo>().FromInstance(_networkAmmo).AsSingle();
            }

            Container.BindInterfacesAndSelfTo<WeaponMuzzle>().AsSingle().WithArguments(_muzzleTransform);
            Container.BindInterfacesTo<LineRendererVfxPlayer>().AsSingle().WithArguments(_trailPrefab);

            Container.BindInterfacesTo<ShootingWeapon>().AsSingle();
            
            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
                Container.BindInterfacesAndSelfTo<KeyboardWeaponTriggersConnector>().AsSingle().NonLazy();

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindInterfacesTo<RaycastShooter>().AsSingle().NonLazy();
                    Container.BindInterfacesTo<ShootingWeaponServerHandler>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.BindInterfacesTo<BulletTrailRenderer>().AsSingle().NonLazy();
                    break;
            }
        }
    }
}