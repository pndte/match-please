using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Network.Variables;
using Bw.UseCases.Character;
using Bw.UseCases.Shooting;
using Bw.UseCases.Shooting.Melee;
using Bw.UseCases.Shooting.View;
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

        [Header("Network")]
        [SerializeField] private NetworkObject _networkObject;
        [SerializeField] private NetworkWeaponHold _weaponHold;
        [SerializeField] private NetworkTransform _networkTransform;

        [Header("Physics")] [SerializeField] private Rigidbody2D _body;

        public override void InstallBindings()
        {
            RequireTurnSpeed();
            RequireStrikeInsideSwing();
            BindConfigs();
            BindMelee();

            var netSchema = new NetEntriesSchemaBuilder();
            HeldWeaponInstaller.Install(Container, _runtimeSettings, netSchema, Parts());
            NetTablesInstaller.Install(Container, _runtimeSettings, netSchema.Build());

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.InstantiateComponent<WeaponHolder>(gameObject);
                    break;
                case PeerType.Client:
                    BindClientVisuals();
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

        private HeldWeaponParts Parts() =>
            new(
                transform,
                _visual,
                _networkObject,
                _weaponHold,
                _networkTransform,
                _body,
                default,
                KeyCode.Alpha1); //TODO: new input system

        private void BindMelee()
        {
            Container.BindInterfacesTo<MeleeSimulator>().AsSingle();
            Container.BindInterfacesTo<MeleeTracer>().AsSingle().WithArguments(transform);
        }

        private void BindClientVisuals()
        {
            Container.Bind<IStateView<WeaponState>>()
                .FromMethod(ctx => new CompositeStateView<WeaponState>(
                    ctx.Container.Instantiate<WeaponStateView>(new object[] { transform }),
                    ctx.Container.Resolve<SeenShots>(),
                    ctx.Container.Instantiate<MeleeRigView>(new object[] { _rig })))
                .AsSingle();
        }

        private void BindConfigs()
        {
            Container.BindInstance(_meleeConfig).AsSingle();
            Container.Bind<IHitConfig>().FromInstance(_meleeConfig).AsSingle();
            Container.BindInstance(_rotationConfig).AsSingle();
            Container.BindInstance(_cameraKickConfig).AsSingle();
            Container.BindInstance(_recoilConfig).AsSingle();
        }
    }
}
