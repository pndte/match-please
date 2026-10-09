using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Variables;
using Bw.Injection.ControlledBy;
using Bw.Injection.Network;
using Bw.Injection.Network.Prediction;
using Bw.Injection.Network.Variables;
using Bw.Injection.Ownership;
using Bw.UseCases.Character;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Character.Network.Prediction;
using Bw.UseCases.Camera.View.Follow;
using Bw.UseCases.Character.View.Audio;
using Bw.UseCases.Character.View.Death;
using Bw.UseCases.Character.View.Hit;
using Bw.UseCases.Movement;
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
        [SerializeField] private Rigidbody2D _physics;
        [SerializeField] private BoxCollider2D _collider;
        [SerializeField] private MovementConfig _movementConfig;
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private HitFeedbackConfig _hitFeedbackConfig;
        [SerializeField] private CharacterDeathConfig _death = new();

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
            Container.Bind<Rigidbody2D>().FromInstance(_physics).AsSingle();
            Container.Bind<BoxCollider2D>().FromInstance(_collider).AsSingle();
            Container.Bind<MovementConfig>().FromInstance(_movementConfig).AsSingle();
            Container.Bind<Animator>().FromInstance(_animator).AsSingle();
            Container.Bind<SpriteRenderer>().FromInstance(_sprite).AsSingle();
            Container.BindInstance(_death).AsSingle();

            var netSchema = new NetEntriesSchemaBuilder();
            OwnershipServicesInstaller.Install(Container, _runtimeSettings, netSchema);
            ControlledByServicesInstaller.Install(Container, _runtimeSettings, netSchema);
            CharacterMovementInstaller.Install(Container, _runtimeSettings, netSchema);
            EventPredictionTargetInstaller<CharacterVitals, float>.Install(Container, _runtimeSettings, netSchema);
            NetTablesInstaller.Install(Container, _runtimeSettings, netSchema.Build());

            BindCharacter();
            Container.InstantiateComponent<ReadonlyCharacterHolder>(gameObject);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<DeadCharacterDespawner>().AsSingle().NonLazy();
                    Container.BindInterfacesTo<VitalsEffectRules>().AsSingle();
                    break;
                case PeerType.Client:
                    Container.BindInterfacesTo<VitalsPredictionRules>().AsSingle();
                    BindClientVisuals();
                    BindClientSounds();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindCharacter()
        {
            Container.Bind(typeof(IReadonlyHealth), typeof(IHealth)).To<Health>().AsSingle()
                .WriterOnlyInto<IHealth>(typeof(MortalCharacter));
            Container.Bind(typeof(IReadonlyCharacter), typeof(IReadonlyAppliedState<CharacterVitals>), typeof(IAppliedState<CharacterVitals>))
                .To<MortalCharacter>().AsSingle()
                .WriterOnlyInto<IAppliedState<CharacterVitals>>(
                    typeof(AuthoritativeState<CharacterVitals, float>), typeof(PredictedState<CharacterVitals, float>));
            Container.Bind<CorpseHitbox>().AsSingle().WithArguments(SingleLayer(_death.CorpseLayer)).NonLazy();
        }

        private static int SingleLayer(LayerMask mask)
        {
            for (var layer = 0; layer < 32; layer++)
                if (mask.value == 1 << layer)
                    return layer;

            throw new ArgumentException($"The corpse layer must be exactly one layer, the mask is {mask.value}.", nameof(mask));
        }

        private void BindClientSounds()
        {
            Container.Bind<BodyHitSound>().AsSingle().WithArguments(transform).NonLazy();
            Container.Bind<CharacterDeathSound>().AsSingle().WithArguments(transform).NonLazy();
        }

        private void BindClientVisuals()
        {
            Container.Bind<HitFeedbackConfig>().FromInstance(_hitFeedbackConfig).AsSingle();
            Container.Bind<HitFlash>().AsSingle().NonLazy();
            Container.Bind<HitCameraShake>().AsSingle().NonLazy();
            Container.Bind<CameraTargetRegistration>().AsSingle().WithArguments(transform).NonLazy();
            Container.Bind<CharacterDeathAnimation>().AsSingle().NonLazy();
            Container.Bind<CharacterDeathBurst>().AsSingle().WithArguments(transform).NonLazy();
        }
    }
}
