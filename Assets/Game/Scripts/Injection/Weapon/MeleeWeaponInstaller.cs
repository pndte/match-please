using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Network.Variables;
using Bw.UseCases.Combat;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Melee;
using Bw.UseCases.Combat.Melee.Network;
using Bw.UseCases.Combat.View;
using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.View.Adapters;
using Bw.UseCases.Combat.View.Adapters.Audio;
using Bw.UseCases.Combat.View.Audio;
using Bw.UseCases.Combat.View.Crosshair;
using Bw.UseCases.Combat.View.Impact;
using Bw.UseCases.Combat.View.Recoil;
using Bw.UseCases.Combat.View.Rig;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Network;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Bw.Injection.Weapon
{
    public class MeleeWeaponInstaller : MonoInstaller
    {
        public const string VisualField = nameof(_visual);
        public const string MeleeConfigField = nameof(_meleeConfig);
        public const string RotationConfigField = nameof(_rotationConfig);
        public const string RecoilConfigField = nameof(_recoilConfig);

        [Inject] private IRuntimeSettings _runtimeSettings;

        [Header("Graphics")] [SerializeField] private Transform _visual;
        [SerializeField] private Animator _rig;

        [Header("Configs")] [SerializeField] private MeleeWeaponConfig _meleeConfig;
        [SerializeField] private WeaponRotationConfig _rotationConfig;
        [SerializeField] private WeaponCameraKickConfig _cameraKickConfig;
        [SerializeField] private WeaponRecoilConfig _recoilConfig;
        [SerializeField] private StrikeFeelConfig _strikeFeelConfig;

        [Header("Network")]
        [SerializeField] private NetworkObject _networkObject;
        [FormerlySerializedAs("_weaponHold")] [SerializeField] private NetworkLifetimedBehaviour _networkLifetimedBehaviour;

        public override void InstallBindings()
        {
            RequireTurnSpeed();
            RequireStrikeInsideSwing();
            RequireStrongStrikeInsideStrongSwing();
            RequireMinChargeWithinFullCharge();
            BindConfigs();
            BindMelee();

            var netSchema = new NetEntriesSchemaBuilder();
            HeldWeaponInstaller<MeleeState>.Install(Container, _runtimeSettings, netSchema, Parts());
            NetTablesInstaller.Install(Container, _runtimeSettings, netSchema.Build());

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.InstantiateComponent<WeaponHolder>(gameObject);
                    break;
                case PeerType.Client:
                    Container.Bind<IReadonlyAimDeflection>().To<SteadyAim>().AsSingle();
                    BindClientVisuals();
                    BindClientSounds();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RequireTurnSpeed()
        {
            if (_meleeConfig.TurnSpeed <= 0f)
                throw new InvalidOperationException(
                    $"Melee config '{_meleeConfig.name}' has no turn speed: the fists would never turn to the cursor.");
        }

        private void RequireStrikeInsideSwing()
        {
            if (_meleeConfig.StrikeTime >= _meleeConfig.SwingTime)
                throw new InvalidOperationException(
                    $"Melee config '{_meleeConfig.name}' strikes after its swing is over: StrikeTime must be shorter than SwingTime.");
        }

        private void RequireStrongStrikeInsideStrongSwing()
        {
            if (_meleeConfig.StrongStrikeTime >= _meleeConfig.StrongSwingTime)
                throw new InvalidOperationException(
                    $"Melee config '{_meleeConfig.name}' strikes after its strong swing is over: StrongStrikeTime must be shorter than StrongSwingTime.");
        }

        private void RequireMinChargeWithinFullCharge()
        {
            if (_meleeConfig.MinChargeTime > _meleeConfig.FullChargeTime)
                throw new InvalidOperationException(
                    $"Melee config '{_meleeConfig.name}' keeps the strong punch charging past its full charge: MinChargeTime must not exceed FullChargeTime.");
        }

        private HeldWeaponParts<MeleeState> Parts() =>
            new(
                transform,
                _visual,
                _networkObject,
                _networkLifetimedBehaviour,
                default,
                WeaponKind.Fists);

        private void BindMelee()
        {
            WeaponSimulatorInstaller<MeleeState, MeleeSimulator>.Install(Container);
            Container.BindInterfacesTo<MeleeTracer>().AsSingle();
            Container.BindInterfacesTo<ChargedShotPower>().AsSingle();
            Container.Bind<StrongSwingRoot>().AsSingle().NonLazy();
        }

        private void BindClientVisuals()
        {
            Container.Bind(typeof(IMeleeSwingsViewModel), typeof(MeleeSwingsViewModel)).To<MeleeSwingsViewModel>().AsSingle();
            Container.Bind<IStateView<MeleeState>>()
                .FromMethod(ctx => new CompositeStateView<MeleeState>(
                    ctx.Container.Resolve<WeaponPoseViewModel<MeleeState>>(),
                    ctx.Container.Resolve<SeenShotsViewModel<MeleeState>>(),
                    ctx.Container.Resolve<MeleeSwingsViewModel>()))
                .AsSingle();
            Container.Bind<MeleeRigView>().AsSingle().WithArguments(_rig).NonLazy();
            Container.Bind<FistsGuardPose>().AsSingle().WithArguments(_rig).NonLazy();
            Container.Bind<BodyPunchPose>().AsSingle().NonLazy();
            Container.Bind<FistsCursor>().AsSingle().NonLazy();
            BindStrikeVisuals();
        }

        private void BindStrikeVisuals()
        {
            Container.BindInstance(_strikeFeelConfig).AsSingle();
            Container.BindInterfacesTo<StrikeImpacts>().AsSingle();
            Container.Bind<StrikeImpactEffectsView>().AsSingle().NonLazy();
            Container.Bind<StrikeHitStop>().AsSingle().WithArguments(_rig).NonLazy();
            Container.Bind<StrikeCameraKick>().AsSingle().NonLazy();
            Container.Bind<FullChargeGlow>().AsSingle().WithArguments(_visual, _visual.GetComponentsInChildren<SpriteRenderer>(true)).NonLazy();
        }

        private void BindClientSounds()
        {
            Container.Bind<MeleeSounds>().FromMethod(ctx => ctx.Container.Resolve<MeleeSoundsCatalog>().For(_meleeConfig)).AsSingle();
            Container.Bind<MeleeSoundsView>().AsSingle().WithArguments(transform).NonLazy();
        }

        private void BindConfigs()
        {
            Container.BindInstance(_meleeConfig).AsSingle();
            Container.BindInstance(_rotationConfig).AsSingle();
            Container.BindInstance(_cameraKickConfig).AsSingle();
            Container.BindInstance(_recoilConfig).AsSingle();
        }
    }
}
