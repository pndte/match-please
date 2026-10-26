using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Prediction.Bubble;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Variables;
using Bw.Entities.Physics;
using Bw.Injection.ControlledBy;
using Bw.Injection.Mortality;
using Bw.Injection.Network.Prediction;
using Bw.Injection.Network.Variables;
using Bw.Injection.Ownership;
using Bw.Injection.Weapon;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Character.View;
using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Character.View.Adapters;
using Bw.UseCases.Character.View.Audio;
using Bw.UseCases.Character.View.Death;
using Bw.UseCases.Character.View.Follow;
using Bw.UseCases.Character.View.Grips;
using Bw.UseCases.Character.View.HealthBar;
using Bw.UseCases.Character.View.Hit;
using Bw.UseCases.Character.View.Player;
using Bw.UseCases.Character.View.Pose;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Characters;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Melee.Network;
using Bw.UseCases.Combat.View.Adapters;
using Bw.UseCases.Combat.View.Adapters.Hit;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Network;
using Bw.UseCases.Mortality.Network.Prediction;
using Bw.UseCases.Mortality.View.Adapters;
using Bw.UseCases.Mortality.View.Death;
using Bw.UseCases.Mortality.View.Guarding;
using Bw.UseCases.Mortality.View.Health;
using Bw.UseCases.Mortality.View.Hit;
using Bw.UseCases.Movement;
using Bw.UseCases.Spawning;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public class CharacterInstaller : MonoInstaller //todo: maybe decompose
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [SerializeField] private HealthConfig _healthConfig;

        [SerializeField] private NetworkObject _networkObject;
        [SerializeField] private NetworkLifetimedBehaviour _networkLifetimedBehaviour;
        [SerializeField] private BodyBox _box;
        [SerializeField] private HoldOval _holdOval;
        [SerializeField] private MovementConfig _movementConfig;
        [SerializeField] private HitFeedbackConfig _hitFeedbackConfig;
        [SerializeField] private DeathConfig _death;
        [SerializeField] private RegenerationConfig _regeneration;
        [SerializeField] private GuardConfig _guard;
        [SerializeField] private SpriteRenderer[] _gripHands;

        public override void InstallBindings()
        {
            Debug.Log("Character installer executed");

            OwnershipInstaller.Install(Container, _runtimeSettings);
            ControlledByInstaller.Install(Container, _runtimeSettings);

            Container.Bind<NetworkObject>().FromInstance(_networkObject).AsSingle();
            Container.Bind<INetworkLifetimedObject>().FromInstance(_networkLifetimedBehaviour).AsSingle().NonLazy();
            var gameObjectLifetime = gameObject.Lifetime();
            Container.BindInstance(gameObjectLifetime).AsSingle();

            Container.Bind<HealthConfig>().FromInstance(_healthConfig).AsSingle();
            Container.Bind<BodyBox>().FromInstance(_box).AsSingle();

            var netSchema = new NetEntriesSchemaBuilder();
            OwnershipServicesInstaller.Install(Container, _runtimeSettings, netSchema);
            ControlledByServicesInstaller.Install(Container, _runtimeSettings, netSchema);
            CharacterMovementInstaller.Install(Container, _runtimeSettings, netSchema, _movementConfig);
            RequireHoldOval();
            MortalInstaller.Install(Container, _runtimeSettings, netSchema, _death, gameObject);
            PushTargetInstaller<IReadonlyMortal, Impacts>.Install(Container, _runtimeSettings);
            GuardInstaller.Install(Container, _runtimeSettings, netSchema, _guard);
            WeaponPickupInstaller.Install(Container, _runtimeSettings, netSchema, transform);
            HandsInstaller.Install(Container, _runtimeSettings, netSchema, _holdOval);
            CarriedWeaponsInstaller.Install(Container, _runtimeSettings, netSchema);
            CharacterControlsInstaller.Install(Container, _runtimeSettings, netSchema);
            NicknameInstaller.Install(Container, _runtimeSettings, netSchema);
            NetTablesInstaller.Install(Container, _runtimeSettings, netSchema.Build());

            Container.BindInterfacesTo<KnockbackRules>().AsSingle();
            BindCharacter();

            if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.Bind<CharacterArming>().AsSingle().NonLazy();
                Container.Bind<Regeneration>().AsSingle().WithArguments(_regeneration).NonLazy();
            }

            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
            {
                BindPrediction();
                BindClientVisuals();
                BindClientSounds();
            }
        }

        private void RequireHoldOval()
        {
            if (_holdOval == null)
                throw new InvalidOperationException("The character has no hold oval: add a HoldOval to its root and assign it to the installer.");

            if (_holdOval.Radii.x <= 0f || _holdOval.Radii.y <= 0f)
                throw new InvalidOperationException($"The character's hold oval needs positive radii, and they are {_holdOval.Radii}.");
        }

        private void BindCharacter()
        {
            Container.Bind(typeof(ICharacter), typeof(IReadonlyCharacter)).To<Character>().AsSingle();
            Container.Bind<CarriersRegistration>().AsSingle().NonLazy();
            Container.Bind<InHandsRegistration>().AsSingle().NonLazy();
        }

        private void BindPrediction()
        {
            Container.Bind<IBubbleMembership>().To<BubbleMembership>().AsSingle().WithArguments(transform);
            Container.Bind<IPredictedBy>().To<CharacterPredictedBy>().AsSingle();
            Container.Bind<CharacterPredictionRegistration>().AsSingle().NonLazy();
            Container.Bind<ControlledCharacterRegistration>().AsSingle().NonLazy();
        }

        private void BindClientSounds()
        {
            Container.Bind<BodyHitSound>().AsSingle().WithArguments(transform).NonLazy();
            Container.Bind<CharacterDeathSound>().AsSingle().WithArguments(transform).NonLazy();
        }

        private void BindClientVisuals()
        {
            Container.Bind(typeof(IHealthViewModel), typeof(IDamageViewModel), typeof(IDeathViewModel)).To<MortalViewModel>().AsSingle();
            Container.Decorate<IDamageViewModel>().With<ArrivingDamageViewModel>();
            Container.Bind<IPlayerViewModel>().To<CharacterPlayerViewModel>().AsSingle();
            Container.Bind(typeof(HitFeedbackConfig), typeof(IHitFlashConfig)).FromInstance(_hitFeedbackConfig).AsSingle();
            Container.Bind<HitFlash>().AsSingle().NonLazy();
            Container.Bind<HitCameraShake>().AsSingle().NonLazy();
            Container.Bind<CameraTargetRegistration>().AsSingle().WithArguments(transform).NonLazy();
            Container.Bind<CharacterDeathAnimation>().AsSingle().NonLazy();
            Container.Bind<CharacterDeathBurst>().AsSingle().WithArguments(transform).NonLazy();
            Container.Bind<CharacterHealthBar>().AsSingle().WithArguments(transform).NonLazy();
            Container.Bind<IGuardViewModel>().To<GuardViewModel>().AsSingle();
            Container.Bind<CharacterGuardBar>().AsSingle().WithArguments(transform).NonLazy();
            Container.Bind<ICharacterPoses>().To<CharacterPoses>().AsSingle();
            Container.Bind<IGripView>().To<GripView>().AsSingle().WithArguments(_gripHands);
            Container.Bind<ICharacterView>().To<CharacterView>().AsSingle().WithArguments(transform);
            Container.Bind<CharacterViewRegistration>().AsSingle().NonLazy();
        }
    }
}
