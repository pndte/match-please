using System;
using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Network;
using Bw.Injection.Network.Prediction;
using Bw.UseCases.Mortality;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Props;
using Bw.UseCases.Props.Network;
using Bw.UseCases.Props.Network.Prediction;
using Bw.UseCases.Props.View.Held;
using Bw.UseCases.Shooting.View.Adapters;
using Bw.UseCases.Shooting.View.Highlight;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Network;
using Bw.UseCases.Shooting.Weapon.Network.Requests;
using Bw.UseCases.Shooting.Weapon.Pickup;
using JetBrains.Core;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Props
{
    public class PropInstaller : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, PropParts, PropInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;
        private readonly PropParts _parts;

        public PropInstaller(
            IRuntimeSettings runtimeSettings,
            INetEntriesSchemaBuilder netSchema,
            PropParts parts)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
            _parts = parts;
        }

        public override void InstallBindings()
        {
            RequireThrowSpeed();
            RequireDamageBounds();
            Container.BindInstance(_parts.Held).AsSingle();
            Container.BindInstance(_parts.Throw).AsSingle();

            BindHeldSimulation();
            BindDrawn();
            BindDropRequest();
            EventPredictionInitiatorInstaller<IReadonlyWeapon, IReadonlyMortal, Impacts>.Install(Container, _runtimeSettings, _netSchema);
            BindHold();

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

        private void RequireThrowSpeed()
        {
            if (_parts.Throw.Speed <= 0f)
                throw new InvalidOperationException(
                    $"Prop throw config '{_parts.Throw.name}' has no throw speed: a thrown prop's damage is scaled by its approach speed over the throw speed.");
        }

        private void RequireDamageBounds()
        {
            if (_parts.Throw.MinDamage > _parts.Throw.MaxDamage)
                throw new InvalidOperationException(
                    $"Prop throw config '{_parts.Throw.name}' keeps no damage between its bounds: MinDamage must not exceed MaxDamage.");
        }

        private void BindHeldSimulation()
        {
            Container.Bind(typeof(IReadonlyWeaponDraw), typeof(IWeaponDraw)).To<WeaponDraw>().AsSingle()
                .WriterOnlyInto<IWeaponDraw>(typeof(WeaponHoldServerHandler), typeof(WeaponDrawReceiver));
            Container.BindInterfacesTo<PropSimulator>().AsSingle();
            Container.BindInterfacesTo<ShootingWeapon<PropState>>().AsSingle().WithArguments(new PropState(0f, 0, true)).NonLazy();

            StreamPredictionInstaller<WeaponInput, PropState>.Install(Container, _runtimeSettings, _netSchema);
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

        private void BindDropRequest()
        {
            var drop = _netSchema.DeclareRequest<Unit>(NetworkDelivery.Reliable);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindRequestReceiver(drop).WhenInjectedInto<WeaponDropServerHandler>();
                    Container.Bind<WeaponDropServerHandler>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.BindRequestSender(drop).WhenInjectedInto<WeaponDropClientHandler>();
                    Container.Bind<WeaponDropClientHandler>().AsSingle().NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindHold()
        {
            Container.Bind<ReleaseSpots>().AsSingle();
            Container.Bind<HeldPropGrip>().AsSingle().WithArguments(_parts.Held.Layer.SingleLayer("held prop"));
            Container.Bind<HeldPropPlacement>().AsSingle();
            Container.Bind(typeof(IStateView<PropState>), typeof(IHeldItem)).To<PropStateView>().AsSingle();
            Container.Bind<HeldItemRegistration>().AsSingle().WithArguments(_parts.Prop).NonLazy();
            Container.Bind<HeldWeaponBody>().AsSingle().NonLazy();
            Container.Decorate<IStateView<RigidState>>().With<UnheldStateView<RigidState>>();
        }

        private void BindServer()
        {
            Container.BindInterfacesTo<WeaponInputPolicy>().AsSingle();
            Container.Bind<WeaponHoldServerHandler>().AsSingle().NonLazy();
            Container.Bind<WeaponKindRegistration>().AsSingle().WithArguments(WeaponKind.Prop).NonLazy(); //TODO: ящик создаёт спавнер ящиков, а не Armory, и в каталоге оружия его нет, поэтому описания у ящика нет: когда матч начнёт засчитывать броски (HitScoring), убийство ящиком бросит исключение в ленте убийств, пока ящику не дадут описание, строку в каталоге, иконку в WeaponIconsConfig и создание через Armory
            Container.BindInterfacesAndSelfTo<ThrownPropHits>().AsSingle();
            Container.Bind<PropRelease>().AsSingle().WithArguments(_parts.Prop).NonLazy();
            Container.Bind<DestroyedPropDrop>().AsSingle().NonLazy();
            Container.Bind<SimulationRewinder<PropState>>().AsSingle();
            Container.Decorate<IRewindable>().With<PropRewindable>();
            Container.Bind<WeaponHolder>().FromNewComponentOn(_parts.Prop.gameObject).AsSingle().NonLazy();
        }

        private void BindClient()
        {
            Container.Bind<ThrowPrediction>().AsSingle().NonLazy();
            Container.BindInterfacesTo<WeaponInputSampler>().AsSingle().WithArguments(_parts.Prop);
            Container.BindInterfacesTo<FloorWeapon>().AsSingle().WithArguments(_parts.Shape);
            Container.Bind<FloorWeaponRegistration>().AsSingle().NonLazy();
            BindClientVisuals();
        }

        private void BindClientVisuals()
        {
            Container.BindInterfacesTo<FloorWeaponHighlightViewModel>().AsSingle();
            Container.Bind<WeaponHighlightView>().AsSingle()
                .WithArguments(_parts.Visual, _parts.Visual.GetComponentsInChildren<SpriteRenderer>(true)).NonLazy();
            Container.BindInterfacesTo<WeaponCarryViewModel>().AsSingle();
            Container.Bind<HeldPropSorting>().AsSingle().WithArguments(_parts.HeldSortingOrder).NonLazy();
        }
    }
}
