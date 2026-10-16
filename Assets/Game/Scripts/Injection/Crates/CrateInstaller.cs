using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.ControlledBy;
using Bw.Injection.Mortality;
using Bw.Injection.Movement;
using Bw.Injection.Network;
using Bw.Injection.Network.Prediction;
using Bw.Injection.Network.Variables;
using Bw.Injection.Ownership;
using Bw.Injection.Props;
using Bw.UseCases.Crates.View.Audio;
using Bw.UseCases.Crates.View.Break;
using Bw.UseCases.Crates.View.Damage;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Network.Prediction;
using Bw.UseCases.Mortality.View.Adapters;
using Bw.UseCases.Mortality.View.Death;
using Bw.UseCases.Mortality.View.Health;
using Bw.UseCases.Mortality.View.Hit;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Network.Prediction;
using Bw.UseCases.Movement.Physics.Abstractions;
using Bw.UseCases.Props;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Network;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Bw.Injection.Crates
{
    public class CrateInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [Header("Network")]
        [SerializeField] private NetworkObject _networkObject;
        [FormerlySerializedAs("_networkLifetimedBehaviour")] [SerializeField] private NetworkWeaponHold _hold;

        [Header("Body")]
        [SerializeField] private Rigidbody2D _physics;
        [SerializeField] private BoxCollider2D _collider;
        [SerializeField] private DynamicBodyConfig _bodyConfig;
        [SerializeField] private HitImpulseConfig _hitImpulse;

        [Header("Mortality")]
        [SerializeField] private HealthConfig _healthConfig;
        [SerializeField] private DeathConfig _death;

        [Header("Visuals")]
        [SerializeField] private Transform _visual;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private HitFlashConfig _hitFlashConfig;
        [SerializeField] private CrateDamageConfig _damage;
        [SerializeField] private int _heldSortingOrder = 4;

        [Header("Prop")]
        [SerializeField] private HeldPropConfig _held;
        [SerializeField] private PropThrowConfig _throw;

        public override void InstallBindings()
        {
            OwnershipInstaller.Install(Container, _runtimeSettings);
            ControlledByInstaller.Install(Container, _runtimeSettings);

            Container.Bind<NetworkObject>().FromInstance(_networkObject).AsSingle();
            Container.Bind(typeof(INetworkLifetimedObject), typeof(IWeaponHold)).FromInstance(_hold).AsSingle().NonLazy();
            Container.BindInstance(gameObject.Lifetime()).AsSingle();

            Container.Bind<HealthConfig>().FromInstance(_healthConfig).AsSingle();
            Container.Bind<Rigidbody2D>().FromInstance(_physics).AsSingle();
            Container.Bind<BoxCollider2D>().FromInstance(_collider).AsSingle();

            var netSchema = new NetEntriesSchemaBuilder();
            OwnershipServicesInstaller.Install(Container, _runtimeSettings, netSchema);
            ControlledByServicesInstaller.Install(Container, _runtimeSettings, netSchema);
            BindBody(netSchema);
            MortalInstaller.Install(Container, _runtimeSettings, netSchema, _death, gameObject);
            PropInstaller.Install(Container, _runtimeSettings, netSchema, new PropParts(transform, _visual, _collider, _held, _throw, _heldSortingOrder));
            NetTablesInstaller.Install(Container, _runtimeSettings, netSchema.Build());

            BindKnockback();

            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
            {
                BindClientVisuals();
                BindClientSounds();
            }
        }

        private void BindBody(INetEntriesSchemaBuilder netSchema) =>
            PredictedBodyInstaller.Install(Container, _runtimeSettings, netSchema, _bodyConfig);

        private void BindKnockback()
        {
            Container.BindInstance(_hitImpulse).AsSingle();
            Container.Bind<IEffectRules<RigidState, Hit>>().To<HitImpulseRules>().AsSingle();

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<SimulationAffectable<RigidState, Hit>>().AsSingle();
                    Container.Bind<IAffectable<Hit>>().To<SimulationAffectable<RigidState, Hit>>().FromResolve()
                        .WhenInjectedInto<AffectableRegistration<IReadonlyMortal, SimulationAffectable<RigidState, Hit>, Hit>>();
                    Container.Bind<AffectableRegistration<IReadonlyMortal, SimulationAffectable<RigidState, Hit>, Hit>>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind(typeof(PredictedRigidEffects<Hit>), typeof(IRigidStateEffects)).To<PredictedRigidEffects<Hit>>().AsSingle();
                    Container.Bind<IPredictionTarget<Hit>>().To<PredictedRigidEffects<Hit>>().FromResolve()
                        .WhenInjectedInto<PredictionTargetRegistration<IReadonlyMortal, PredictedRigidEffects<Hit>, Hit>>();
                    Container.Bind<PredictionTargetRegistration<IReadonlyMortal, PredictedRigidEffects<Hit>, Hit>>().AsSingle().NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindClientVisuals()
        {
            Container.Bind<SpriteRenderer>().FromInstance(_sprite).AsSingle();
            Container.Bind(typeof(IHealthViewModel), typeof(IDamageViewModel), typeof(IDeathViewModel)).To<MortalViewModel>().AsSingle();
            Container.Bind<IHitFlashConfig>().FromInstance(_hitFlashConfig).AsSingle();
            Container.Bind<HitFlash>().AsSingle().NonLazy();
            Container.BindInstance(_damage).AsSingle();
            Container.Bind<CrateCracks>().AsSingle().NonLazy();
            Container.Bind<CrateHitJolt>().AsSingle().WithArguments(_visual).NonLazy();
            Container.Bind<CrateBreak>().AsSingle().WithArguments(transform).NonLazy();
        }

        private void BindClientSounds()
        {
            Container.Bind<CrateHitSound>().AsSingle().WithArguments(transform).NonLazy();
            Container.Bind<CrateBreakSound>().AsSingle().WithArguments(transform).NonLazy();
        }
    }
}
