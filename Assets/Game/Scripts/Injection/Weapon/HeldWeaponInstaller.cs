using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.ControlledBy;
using Bw.Injection.Network;
using Bw.Injection.Network.Prediction;
using Bw.Injection.Ownership;
using Bw.UseCases.Mortality;
using Bw.UseCases.Shooting;
using Bw.UseCases.Shooting.View;
using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Shooting.View.Crosshair;
using Bw.UseCases.Shooting.View.Recoil;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Network;
using Bw.UseCases.Shooting.Weapon.Network.Prediction;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Weapon
{
    public class HeldWeaponInstaller<TState> : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, HeldWeaponParts<TState>, HeldWeaponInstaller<TState>>
        where TState : struct, IWeaponState, IEquatable<TState>
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
            ControlledByInstaller.Install(Container, _runtimeSettings);

            Container.Bind<NetworkObject>().FromInstance(_parts.NetworkObject).AsSingle();
            Container.Bind(typeof(INetworkLifetimedObject), typeof(IWeaponHold)).FromInstance(_parts.Hold).AsSingle();
            Container.BindInstance(_parts.Weapon.gameObject.Lifetime()).AsSingle();

            OwnershipServicesInstaller.Install(Container, _runtimeSettings, _netSchema);
            ControlledByServicesInstaller.Install(Container, _runtimeSettings, _netSchema);

            BindWeapon();
            BindDrawn();
            EventPredictionInitiatorInstaller<IReadonlyWeapon, IReadonlyMortal, Hit>.Install(Container, _runtimeSettings, _netSchema);

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
            Container.Bind(typeof(IReadonlyWeaponDraw), typeof(IWeaponDraw)).To<WeaponDraw>().AsSingle()
                .WriterOnlyInto<IWeaponDraw>(typeof(WeaponHoldServerHandler), typeof(WeaponDrawReceiver));
            Container.BindInterfacesTo<ShootingWeapon<TState>>().AsSingle().WithArguments(_parts.Initial).NonLazy();

            StreamPredictionInstaller<WeaponInput, TState>.Install(Container, _runtimeSettings, _netSchema);
        }

        private void BindDrawn()
        {
            var drawn = _netSchema.DeclareResult<bool>(NetworkDelivery.Reliable);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindResultSender(drawn).WhenInjectedInto(typeof(WeaponDrawBroadcaster), typeof(LateJoinWeaponDrawSender));
                    Container.Bind<WeaponDrawBroadcaster>().AsSingle().NonLazy();
                    Container.Bind<LateJoinWeaponDrawSender>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.BindResultReceiver(drawn).WhenInjectedInto<WeaponDrawReceiver>();
                    Container.Bind<WeaponDrawReceiver>().AsSingle().NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindServer()
        {
            Container.BindInterfacesTo<WeaponInputPolicy>().AsSingle();
            Container.Bind<IStateView<TState>>().To<WeaponStateView<TState>>().AsSingle().WithArguments(_parts.Weapon);
            Container.Bind<RaycastShooter<TState>>().AsSingle().NonLazy();
            Container.Bind<WeaponHoldServerHandler>().AsSingle().NonLazy();
            Container.Bind<ReleasedWeaponTeleport>().AsSingle().WithArguments(_parts.NetworkTransform).NonLazy();
            Container.Bind<HeldWeaponBody>().AsSingle().WithArguments(_parts.Body).NonLazy();
            Container.Bind<WeaponKindRegistration>().AsSingle().WithArguments(_parts.Kind).NonLazy();
        }

        private void BindClient()
        {
            Container.BindInterfacesTo<WeaponInputSampler>().AsSingle().WithArguments(_parts.Weapon);
            Container.Bind<HeldWeaponNetworkTransform>().AsSingle().WithArguments(_parts.NetworkTransform).NonLazy();
            Container.Bind<HitPredictor<TState>>().AsSingle().NonLazy();
            BindClientVisuals();
        }

        private void BindClientVisuals()
        {
            Container.Bind(typeof(ISeenShots), typeof(SeenShots<TState>)).To<SeenShots<TState>>().AsSingle();
            Container.Bind<StowedWeaponView>().AsSingle().WithArguments(_parts.Visual.GetComponentsInChildren<Renderer>(true)).NonLazy();
            Container.Bind<WeaponRecoilView>().AsSingle().WithArguments(_parts.Visual).NonLazy();
            Container.Bind<WeaponCameraKick>().AsSingle().NonLazy();
            Container.Bind<WeaponCursorKick>().AsSingle().NonLazy();
        }

        private void BindVisualFlip()
        {
            Container.Bind<WeaponVisualFlip>().AsSingle().WithArguments(_parts.Weapon, _parts.Visual).NonLazy();
        }
    }
}
