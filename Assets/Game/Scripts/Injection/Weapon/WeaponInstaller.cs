using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.ControlledBy;
using Bw.Injection.Network;
using Bw.Injection.Network.Prediction;
using Bw.Injection.Network.Variables;
using Bw.Injection.Ownership;
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
using Bw.UseCases.Shooting.Weapon.Network.Prediction;
using Bw.UseCases.Shooting.Weapon.Network.Requests;
using JetBrains.Core;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Weapon
{
    public class WeaponInstaller : MonoInstaller // todo: decompose
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [Header("Graphics")] [SerializeField] private Transform _visual;
        [SerializeField] private Animator _rig;

        [Header("WeaponMuzzle")] [SerializeField] private Transform _muzzleTransform;

        [Header("Configs")] [SerializeField] private RaycastShootConfig _raycastShootConfig;
        [SerializeField] private ShootingWeaponConfig _shootingWeaponConfig;
        [SerializeField] private WeaponRotationConfig _rotationConfig;
        [SerializeField] private ShotVfxConfig _vfxConfig;
        [SerializeField] private WeaponCameraKickConfig _cameraKickConfig;

        [Header("Network")]
        [SerializeField] private NetworkObject _networkObject;
        [SerializeField] private NetworkWeaponHold _weaponHold;
        [SerializeField] private NetworkTransform _networkTransform;

        [Header("Physics")] [SerializeField] private Rigidbody2D _body;

        public override void InstallBindings()
        {
            OwnershipInstaller.Install(Container, _runtimeSettings);
            ControlledByInstaller.Install(Container, _runtimeSettings);

            Container.Bind<NetworkObject>().FromInstance(_networkObject).AsSingle();
            Container.Bind(typeof(INetworkLifetimedObject), typeof(IWeaponHold)).FromInstance(_weaponHold).AsSingle();
            BindLifetime();

            var netSchema = new NetEntriesSchemaBuilder();
            OwnershipServicesInstaller.Install(Container, _runtimeSettings, netSchema);
            ControlledByServicesInstaller.Install(Container, _runtimeSettings, netSchema);

            BindConfigs();
            BindWeapon(netSchema);
            BindDropRequest(netSchema);
            EventPredictionInitiatorInstaller<IReadonlyWeapon, IReadonlyCharacter, Hit>.Install(Container, _runtimeSettings, netSchema);
            NetTablesInstaller.Install(Container, _runtimeSettings, netSchema.Build());

            BindVisualFlip();

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

        private void BindWeapon(INetEntriesSchemaBuilder netSchema)
        {
            Container.BindInterfacesTo<WeaponSimulator>().AsSingle();
            Container.BindInterfacesTo<ShootingWeapon>().AsSingle().NonLazy();
            Container.BindInterfacesTo<WeaponMuzzle>().AsSingle().WithArguments(_muzzleTransform);
            Container.BindInterfacesTo<ShotTracer>().AsSingle().WithArguments(transform);

            StreamPredictionInstaller<WeaponInput, WeaponState>.Install(Container, _runtimeSettings, netSchema);
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
            Container.BindInterfacesTo<WeaponInputPolicy>().AsSingle();
            Container.Bind<IStateView<WeaponState>>().To<WeaponStateView>().AsSingle().WithArguments(transform);
            Container.Bind<RaycastShooter>().AsSingle().NonLazy();
            Container.Bind<WeaponHoldServerHandler>().AsSingle().NonLazy();
            Container.Bind<WeaponDropServerHandler>().AsSingle().NonLazy();
            Container.Bind<UnheldWeaponDespawner>().AsSingle().NonLazy();
            Container.Bind<HeldWeaponBody>().AsSingle().WithArguments(_body).NonLazy();
            Container.InstantiateComponent<WeaponHolder>(gameObject);
        }

        private void BindClient()
        {
            Container.BindInterfacesTo<WeaponInputSampler>().AsSingle().WithArguments(transform);
            Container.Bind<HeldWeaponNetworkTransform>().AsSingle().WithArguments(_networkTransform).NonLazy();
            Container.Bind<WeaponDropClientHandler>().AsSingle().NonLazy();
            Container.Bind<HitPredictor>().AsSingle().NonLazy();
            BindClientVisuals();
            BindClientSounds();
        }

        private void BindClientVisuals()
        {
            Container.Bind<IStateView<WeaponState>>()
                .FromMethod(ctx => new CompositeStateView<WeaponState>(
                    ctx.Container.Instantiate<WeaponStateView>(new object[] { transform }),
                    ctx.Container.Instantiate<WeaponShotEffectsView>(),
                    ctx.Container.Instantiate<WeaponReloadCursor>(),
                    ctx.Container.Instantiate<WeaponReloadSoundsView>(new object[] { transform }),
                    ctx.Container.Instantiate<WeaponRigView>(new object[] { _rig })))
                .AsSingle();
            Container.Bind<WeaponCameraKick>().AsSingle().NonLazy();
            Container.Bind<WeaponCursorKick>().AsSingle().NonLazy();
        }

        private void BindClientSounds()
        {
            Container.Bind<WeaponSoundsConfig>().FromMethod(ctx => ctx.Container.Resolve<WeaponSoundsCatalog>().For(_shootingWeaponConfig)).AsSingle();
            Container.BindInterfacesTo<ShotSfxPlayer>().AsSingle();
        }

        private void BindVisualFlip()
        {
            Container.Bind<WeaponVisualFlip>().AsSingle().WithArguments(transform, _visual).NonLazy();
        }

        private void BindConfigs()
        {
            Container.BindInstance(_raycastShootConfig).AsSingle();
            Container.BindInstance(_vfxConfig).AsSingle();
            Container.BindInstance(_shootingWeaponConfig).AsSingle();
            Container.Bind<IHitConfig>().FromInstance(_shootingWeaponConfig).AsSingle();
            Container.BindInstance(_shootingWeaponConfig.AmmoSettings).AsSingle();
            Container.BindInstance(_rotationConfig).AsSingle();
            Container.BindInstance(_cameraKickConfig).AsSingle();
        }

        private void BindLifetime()
        {
            var gameObjectLifetime = gameObject.Lifetime();
            Container.BindInstance(gameObjectLifetime).AsSingle();
        }
    }
}
