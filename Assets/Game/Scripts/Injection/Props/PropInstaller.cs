using System;
using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Network;
using Bw.Injection.Network.Prediction;
using Bw.Injection.Weapon;
using Bw.UseCases.Character.View.Grips;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands.Abstractions;
using Bw.UseCases.Combat.View.Adapters;
using Bw.UseCases.Combat.View.Highlight;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Combat.Weapon.Pickup;
using Bw.UseCases.Mortality;
using Bw.UseCases.Movement;
using Bw.UseCases.Props;
using Bw.UseCases.Props.Network;
using Bw.UseCases.Props.Network.Prediction;
using Bw.UseCases.Props.View;
using Bw.UseCases.Props.View.Abstractions;
using Bw.UseCases.Props.View.Adapters;
using Bw.UseCases.Props.View.Aim;
using Bw.UseCases.Props.View.Audio;
using Bw.UseCases.Props.View.Flight;
using Bw.UseCases.Props.View.Held;
using Bw.UseCases.Props.View.Impact;
using Bw.UseCases.Props.View.Throw;
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
            RequireWindUpSwing();
            Container.BindInstance(_parts.Held).AsSingle();
            Container.BindInstance(_parts.Throw).AsSingle();

            BindHeldSimulation();
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

        private void RequireWindUpSwing()
        {
            var swing = _parts.Held.WindUpSwing;
            if (swing == null || swing.length < 2)
                throw new InvalidOperationException(
                    $"Held prop config '{_parts.Held.name}' has no wind-up swing: the curve needs keys from the press to the release.");

            if (Mathf.Abs(swing.Evaluate(0f)) > 0.01f)
                throw new InvalidOperationException(
                    $"Held prop config '{_parts.Held.name}' starts its wind-up swing at {swing.Evaluate(0f)}: the swing starts at 0, or the prop jumps at the press.");
        }

        private void BindHeldSimulation()
        {
            WeaponSimulatorInstaller<PropState, PropSimulator>.Install(Container);
            Container.BindInterfacesTo<ShootingWeapon<PropState>>().AsSingle().WithArguments(new PropState(0f, PropPhase.Holding, 0, 0, true)).NonLazy();
            Container.Bind<WeaponKindRegistration>().AsSingle().WithArguments(WeaponKind.Prop).NonLazy(); //TODO: ящик создаёт спавнер ящиков, а не Armory, и в каталоге оружия его нет, поэтому описания у ящика нет: когда матч начнёт засчитывать броски (HitScoring), убийство ящиком бросит исключение в ленте убийств, пока ящику не дадут описание, строку в каталоге, иконку в WeaponIconsConfig и создание через Armory
            Container.Bind<WeaponNetworkRegistration>().AsSingle().NonLazy();
            Container.Bind<WeaponClock>().AsSingle().NonLazy();

            PredictionStateInstaller<WeaponInput, PropState>.Install(Container, _runtimeSettings, _netSchema);
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
            Container.Bind<HeldPropPlacement>().AsSingle();
            Container.Bind<PropSwing>().AsSingle();
            Container.Bind(typeof(IHeldBox), typeof(HeldPropPose)).To<HeldPropPose>().AsSingle().WithArguments(HeldPropLayer);
            Container.Bind<HeldBoxRegistration>().AsSingle().NonLazy();
            Container.Bind<ReleaseSpots>().AsSingle();
            Container.Bind<ThrowLaunch>().AsSingle();
            Container.Bind<HeldWeaponBody>().AsSingle().NonLazy();
            Container.Decorate<IStateView<RigidState>>().With<UnheldStateView<RigidState>>();
        }

        private int HeldPropLayer => _parts.Held.Layer.SingleLayer("held prop");

        private void BindServer()
        {
            Container.Bind<IStateView<PropState>>().To<HeldPropPose>().FromResolve();
            Container.Bind<SimulationViewSync<PropState>>().AsSingle().NonLazy();
            Container.Bind<CarrierOwnership>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ThrownPropHits>().AsSingle();
            Container.Bind<PropLauncher>().AsSingle();
            Container.Bind<PropRelease>().AsSingle().NonLazy();
            Container.Bind<DestroyedPropDrop>().AsSingle().NonLazy();
            Container.Bind<SimulationRewinder<PropState>>().AsSingle();
            Container.Decorate<IRewindable>().With<PropRewindable>();
            Container.Bind<WeaponHolder>().FromNewComponentOn(_parts.Prop.gameObject).AsSingle().NonLazy();
        }

        private void BindClient()
        {
            Container.Bind<ThrowConfirmation>().AsSingle();
            Container.Bind<ThrowPrediction>().AsSingle().NonLazy();
            Container.Bind<IFedInputs<WeaponInput>>().To<WeaponFeedLog>().AsSingle();
            Container.Bind<IReadonlyAimDeflection>().To<SteadyAim>().AsSingle();
            Container.Bind<AimDeflectionRegistration>().AsSingle().NonLazy();
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
            Container.Bind<IHeldPropViewModel>().To<HeldPropViewModel>().AsSingle();
            Container.Bind<HeldPropView>().AsSingle().WithArguments(_parts.Prop).NonLazy();
            Container.Bind<HeldPropSorting>().AsSingle().WithArguments(_parts.HeldSortingOrder).NonLazy();
            Container.Bind<HeldGripsRegistration>().AsSingle()
                .WithArguments(_parts.Prop.GetComponentsInChildren<HandGrip>(true)).NonLazy();
            BindThrowVisuals();
            BindFlightVisuals();
            Container.Bind<PropSoundsView>().AsSingle().WithArguments(_parts.Prop).NonLazy();
        }

        private void BindThrowVisuals()
        {
            Container.Bind(typeof(IPropThrowsViewModel), typeof(PropThrowsViewModel)).To<PropThrowsViewModel>().AsSingle();
            Container.Bind(typeof(IThrowArcViewModel), typeof(ThrowArcViewModel)).To<ThrowArcViewModel>().AsSingle();
            Container.Bind<IStateView<PropState>>()
                .FromMethod(context => new CompositeStateView<PropState>(
                    context.Container.Resolve<HeldPropPose>(),
                    context.Container.Resolve<PropThrowsViewModel>(),
                    context.Container.Resolve<ThrowArcViewModel>()))
                .AsSingle();
            Container.Bind<ThrowBodyPose>().AsSingle().NonLazy();
            Container.Bind<ThrowCameraKick>().AsSingle().NonLazy();
            Container.Bind<ThrowTrajectory>().AsSingle();
            Container.Bind<ThrowArcRegistration>().AsSingle().NonLazy();
        }

        private void BindFlightVisuals()
        {
            Container.Bind<PropMotion>().AsSingle();
            Container.Bind<PropFlightFx>().FromMethod(context => FlightFx(context.Container)).AsSingle();
            Container.Bind<PropFlightView>().AsSingle().WithArguments(_parts.Visual.GetComponentInChildren<SpriteRenderer>(true)).NonLazy();
            Container.Bind<PropImpactView>().AsSingle().WithArguments(_parts.Visual).NonLazy();
        }

        private PropFlightFx FlightFx(DiContainer container)
        {
            var fx = container.InstantiatePrefabForComponent<PropFlightFx>(container.Resolve<PropVfxConfig>().FlightFx, _parts.Prop);
            var size = _parts.Shape.Size;
            fx.transform.localPosition = _parts.Shape.Offset;
            fx.Trail.widthMultiplier *= Mathf.Max(size.x, size.y);
            var lines = fx.Lines.shape;
            lines.scale = Vector3.Scale(lines.scale, new Vector3(size.x, size.y, 1f));
            return fx;
        }
    }
}
