using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Prediction.Bubble;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Variables;
using Bw.Injection.Network.Prediction;
using Bw.Injection.Ownership;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.View;
using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.View.Adapters;
using Bw.UseCases.Combat.View.Crosshair;
using Bw.UseCases.Combat.View.Recoil;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Network.Prediction;
using Bw.UseCases.Mortality;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Weapon
{
    public class HeldWeaponInstaller<TState> : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, HeldWeaponParts<TState>, HeldWeaponInstaller<TState>>
        where TState : struct, IWeaponState, IAimedState<TState>, IEquatable<TState>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;
        private readonly HeldWeaponParts<TState> _parts;

        public HeldWeaponInstaller(
            IRuntimeSettings runtimeSettings,
            INetEntriesSchemaBuilder netSchema,
            HeldWeaponParts<TState> parts)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
            _parts = parts;
        }

        public override void InstallBindings()
        {
            OwnershipInstaller.Install(Container, _runtimeSettings);

            Container.Bind<NetworkObject>().FromInstance(_parts.NetworkObject).AsSingle();
            Container.Bind<INetworkLifetimedObject>().FromInstance(_parts.NetworkLifetimed).AsSingle();
            Container.BindInstance(_parts.Weapon.gameObject.Lifetime()).AsSingle();

            OwnershipServicesInstaller.Install(Container, _runtimeSettings, _netSchema);

            BindWeapon();
            EventPredictionInitiatorInstaller<IReadonlyWeapon, IReadonlyMortal, Impacts>.Install(Container, _runtimeSettings, _netSchema);

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

        private void BindWeapon()
        {
            Container.BindInterfacesTo<ShootingWeapon<TState>>().AsSingle().WithArguments(_parts.Initial).NonLazy();
            Container.Bind<WeaponNetworkRegistrar>().AsSingle().NonLazy();
            Container.Bind<WeaponClock>().AsSingle().NonLazy();
            Container.Bind<WeaponOrbit>().AsSingle();
            Container.Bind<ShotTimer>().AsSingle();

            RollbackStateInstaller<TState>.Install(Container, _runtimeSettings, _netSchema);
        }

        private void BindServer()
        {
            Container.Bind<RaycastShooter<TState>>().AsSingle().NonLazy();
            Container.Bind<CarrierOwnerAssigner>().AsSingle().NonLazy();
        }

        private void BindClient()
        {
            Container.Bind<IBubbleMembership>().To<BubbleMembership>().AsSingle().WithArguments(_parts.Weapon);
            Container.Bind<IPredictedBy>().To<WeaponPredictedBy>().AsSingle();
            Container.Bind<IReadonlyControlledBy>().To<HolderControl>().AsSingle();
            Container.Bind<IStateDivergence<TState>>().To<ExactDivergence<TState>>().AsSingle();
            Container.Bind<AimDeflectionRegistrar>().AsSingle().NonLazy();
            Container.Bind<IStrikes>().To<HitPredictor<TState>>().AsSingle().NonLazy();
            Container.Bind<DecidedImpacts>().AsSingle().NonLazy();
            Container.Bind<StrikeDamagePredictor>().AsSingle().NonLazy();
            BindClientVisuals();
        }

        private void BindClientVisuals()
        {
            Container.BindInterfacesTo<WeaponCarryViewModel>().AsSingle();
            Container.Bind(typeof(IShotsViewModel), typeof(SeenShotsViewModel<TState>)).To<SeenShotsViewModel<TState>>().AsSingle();
            Container.Bind(typeof(IWeaponPoseViewModel), typeof(WeaponPoseViewModel<TState>)).To<WeaponPoseViewModel<TState>>().AsSingle();
            Container.Bind<WeaponOrbitView>().AsSingle().WithArguments(_parts.Weapon).NonLazy();
            Container.Bind<StowedWeaponView>().AsSingle().WithArguments(_parts.Visual.GetComponentsInChildren<Renderer>(true)).NonLazy();
            Container.Bind<WeaponRecoilView>().AsSingle().WithArguments(_parts.Visual).NonLazy();
            Container.Bind<WeaponCameraKickView>().AsSingle().NonLazy();
            Container.Bind<WeaponCursorKickView>().AsSingle().NonLazy();
        }

        private void BindVisualFlip()
        {
            Container.Bind<WeaponVisualFlipper>().AsSingle().WithArguments(_parts.Weapon, _parts.Visual).NonLazy();
        }
    }
}
