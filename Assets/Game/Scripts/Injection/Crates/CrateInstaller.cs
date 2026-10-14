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
using Bw.UseCases.Mortality.View.Hit;
using Bw.UseCases.Movement;
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
        [SerializeField] private BodyConfig _bodyConfig;

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
            PushTargetInstaller<IReadonlyMortal, BodyState, Hit>.Install(Container, _runtimeSettings);
            PropInstaller.Install(Container, _runtimeSettings, netSchema, new PropParts(transform, _visual, _collider, _held, _throw, _heldSortingOrder));
            NetTablesInstaller.Install(Container, _runtimeSettings, netSchema.Build());

            Container.BindInterfacesTo<KnockbackRules>().AsSingle();

            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
            {
                BindClientVisuals();
                BindClientSounds();
            }
        }

        private void BindBody(INetEntriesSchemaBuilder netSchema)
        {
            Container.Bind<IStateView<BodyState>>().To<BodyStateView<BodyState>>().AsSingle();
            ShovableInstaller<BodyState>.Install(Container, _runtimeSettings);
            BodyInstaller<BodyState>.Install(Container, _runtimeSettings, _bodyConfig);
            ReplicationInstaller<BodyState>.Install(Container, _runtimeSettings, netSchema);

            if (_runtimeSettings.CurrentPeerType == PeerType.Server)
                Container.Bind(typeof(IReadonlySimulation<BodyState>), typeof(ISimulation<BodyState>), typeof(ISteppable), typeof(IRestartable<BodyState>), typeof(IShovable))
                    .To<FreeBody>().AsSingle()
                    .WriterOnlyInto<ISimulation<BodyState>>(typeof(SimulationAffectable<,>))
                    .NonLazy();
        }

        private void BindClientVisuals()
        {
            Container.Bind<SpriteRenderer>().FromInstance(_sprite).AsSingle();
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
