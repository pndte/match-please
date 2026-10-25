using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Prediction.Bubble;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Physics;
using Bw.Entities.Simulation;
using Bw.Injection.Level;
using Bw.Injection.Match;
using Bw.UseCases.Character;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Characters;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Hands.Abstractions;
using Bw.UseCases.Combat.View.Adapters;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Pickup;
using Bw.UseCases.Combat.Weapon.Pickup.Abstractions;
using Bw.UseCases.Level;
using Bw.UseCases.Match;
using Bw.UseCases.Movement.Network.Prediction;
using Bw.UseCases.Movement.Physics;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public class GameSessionInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [SerializeField] private MatchConfig _match;

        public override void InstallBindings()
        {
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

        private void BindServer()
        {
            BindPhysics();
            Container.Bind<IWorldStep>().To<WorldStep>().AsSingle().WithArguments(gameObject.Lifetime()).NonLazy();
            Container.BindInterfacesTo<GameObjectByCharacterCollection>().AsSingle();
            Container.BindInterfacesTo<MovingPlatformCollection>().AsSingle();
            Container.BindInterfacesTo<ShovableCollection>().AsSingle();
            BindWeapons();
            Container.BindInterfacesTo<LagCompensator>().AsSingle();
            Container.Bind<IClientBubbles>().To<ClientBubbles>().AsSingle().WithArguments(gameObject.Lifetime());
            Container.BindInterfacesTo<RaycastShotResolver>().AsSingle().NonLazy();
            Container.Bind<IControllerSamplers<CharacterControls>>().To<ControllerSamplers<CharacterControls>>().AsSingle();
            MatchSessionInstaller.Install(Container, _match);
        }

        private void BindClient()
        {
            BindPhysics();
            Container.Bind(typeof(IWorldStep), typeof(IRollbackParts)).To<WorldRollback>().AsSingle().WithArguments(gameObject.Lifetime()).NonLazy();
            Container.Bind<IInputDelay>().To<AdaptiveInputDelay>().AsSingle().WithArguments(gameObject.Lifetime()).NonLazy();
#if UNITY_EDITOR
            Container.Bind<InputDelayLog>().AsSingle().WithArguments(gameObject.Lifetime()).NonLazy();
#endif
            Container.Bind<IControlledCharacters>().To<ControlledCharacters>().AsSingle();
            Container.Bind<IPredictionBubble>().To<PredictionBubble>().AsSingle();
            Container.BindInterfacesTo<MovingPlatformCollection>().AsSingle();
            Container.BindInterfacesTo<ShovableCollection>().AsSingle();
            BindWeapons();
            Container.Bind<IAimDeflections>().To<AimDeflections>().AsSingle();
            Container.Bind<ICharacterPredictions>().To<CharacterPredictions>().AsSingle();
            Container.Bind(typeof(IReadonlyFloorWeapons), typeof(IFloorWeapons)).To<FloorWeapons>().AsSingle()
                .WriterOnlyInto<IFloorWeapons>(typeof(FloorWeaponRegistration));
            BindClientVisuals();
        }

        private void BindClientVisuals()
        {
            Container.Bind<ICharacterViews>().To<CharacterViews>().AsSingle();
        }

        private void BindWeapons()
        {
            Container.Bind(typeof(IReadonlyWeaponKindRegistry), typeof(IWeaponKindRegistry)).To<WeaponKindRegistry>().AsSingle()
                .WriterOnlyInto<IWeaponKindRegistry>(typeof(WeaponKindRegistration));
            Container.Bind<IWeaponCarriers>().To<WeaponCarriers>().AsSingle();
            Container.Bind<INetworkWeapons>().To<NetworkWeapons>().AsSingle();
            Container.Bind<ICharacterTriggers>().To<CharacterTriggers>().AsSingle();
            Container.Bind<IWeaponsInHands>().To<WeaponsInHands>().AsSingle();
            Container.Bind<IHeldBoxes>().To<HeldBoxes>().AsSingle();
        }

        private void BindPhysics()
        {
            RequireNoClassicColliders();
            Container.Bind(typeof(IPhysicsSpace), typeof(PhysicsSpace)).To<PhysicsSpace>()
                .FromMethod(context => PhysicsSpace.Create(
                    gameObject.Lifetime(),
                    context.Container.Resolve<PhysicsConfig>(),
                    context.Container.Resolve<ISimulationStep>()))
                .AsSingle();
            Container.Bind<IPhysicsSteps>().To<PhysicsSpace>().FromResolve();
            Container.Bind<PhysicsSchedule>()
                .FromMethod(context => context.Container.Instantiate<PhysicsSchedule>(new object[] { gameObject.Lifetime(), context.Container.Resolve<PhysicsSpace>() }))
                .AsSingle()
                .NonLazy();
            Container.Bind<LevelGeometry>()
                .FromMethod(context => LevelGeometry.Create(gameObject.Lifetime(), context.Container.Resolve<IPhysicsSpace>(), LevelBoxes.In(gameObject.scene)))
                .AsSingle()
                .NonLazy();
        }

        private void RequireNoClassicColliders()
        {
            foreach (var collider in FindObjectsByType<Collider2D>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (collider.enabled && collider.gameObject.scene == gameObject.scene)
                    throw new InvalidOperationException($"'{collider.name}' has an enabled {collider.GetType().Name}, which collides with nothing: classic 2D physics is off, the level is made of BodyBoxes.");
        }
    }
}