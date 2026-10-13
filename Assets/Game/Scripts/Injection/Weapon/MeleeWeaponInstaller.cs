using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Network.Variables;
using Bw.UseCases.Shooting;
using Bw.UseCases.Shooting.Melee;
using Bw.UseCases.Shooting.Melee.Network;
using Bw.UseCases.Shooting.View;
using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Shooting.View.Audio;
using Bw.UseCases.Shooting.View.Crosshair;
using Bw.UseCases.Shooting.View.Impact;
using Bw.UseCases.Shooting.View.Recoil;
using Bw.UseCases.Shooting.View.Rig;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Network;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
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
        [SerializeField] private NetworkWeaponHold _weaponHold;
        [SerializeField] private NetworkTransform _networkTransform;

        [Header("Physics")] [SerializeField] private Rigidbody2D _body;

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
                _weaponHold,
                _networkTransform,
                _body,
                default,
                WeaponKind.Fists);

        private void BindMelee()
        {
            Container.BindInterfacesTo<MeleeSimulator>().AsSingle();
            Container.BindInterfacesTo<MeleeTracer>().AsSingle().WithArguments(transform);
            Container.BindInterfacesTo<ChargedShotPower>().AsSingle();
            Container.Bind<StrongSwingRoot>().AsSingle().WithArguments(transform).NonLazy();
        }

        private void BindClientVisuals()
        {
            Container.Bind(typeof(IMeleeSwings), typeof(MeleeSwings)).To<MeleeSwings>().AsSingle();
            Container.Bind<IStateView<MeleeState>>()
                .FromMethod(ctx => new CompositeStateView<MeleeState>(
                    ctx.Container.Instantiate<WeaponStateView<MeleeState>>(new object[] { transform }),
                    ctx.Container.Resolve<SeenShots<MeleeState>>(),
                    ctx.Container.Resolve<MeleeSwings>()))
                .AsSingle();
            Container.Bind<MeleeRigView>().AsSingle().WithArguments(_rig).NonLazy();
            Container.Bind<BodyPunchPose>().AsSingle().WithArguments(transform).NonLazy();
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
            Container.Bind<MeleeSoundsConfig>().FromMethod(ctx => ctx.Container.Resolve<MeleeSoundsCatalog>().For(_meleeConfig)).AsSingle();
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
