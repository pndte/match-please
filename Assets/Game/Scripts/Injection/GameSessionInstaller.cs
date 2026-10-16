using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Physics;
using Bw.Entities.Simulation;
using Bw.Injection.Match;
using Bw.UseCases.Character;
using Bw.UseCases.Level;
using Bw.UseCases.Match;
using Bw.UseCases.Movement.Network.Prediction;
using Bw.UseCases.Movement.Physics;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Network;
using Bw.UseCases.Shooting.Weapon.Pickup;
using Bw.UseCases.Shooting.Weapon.Pickup.Abstractions;
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
            Container.Bind<IPhysicsSteps>().To<PhysicsSpace>().FromResolve();
            Container.Bind<PassiveStepper>()
                .FromMethod(context => new PassiveStepper(gameObject.Lifetime(), context.Container.Resolve<INetworkTicks>(), context.Container.Resolve<PhysicsSpace>()))
                .AsSingle()
                .NonLazy();
            Container.BindInterfacesTo<GameObjectByCharacterCollection>().AsSingle();
            Container.BindInterfacesTo<MovingPlatformCollection>().AsSingle();
            Container.BindInterfacesTo<ShovableCollection>().AsSingle();
            Container.BindInterfacesTo<ScheduledCollection>().AsSingle();
            Container.Bind(typeof(IReadonlyWeaponKindRegistry), typeof(IWeaponKindRegistry)).To<WeaponKindRegistry>().AsSingle()
                .WriterOnlyInto<IWeaponKindRegistry>(typeof(WeaponKindRegistration));
            Container.BindInterfacesTo<WeaponInventories>().AsSingle();
            Container.BindInterfacesTo<RewindableCollection>().AsSingle();
            Container.BindInterfacesTo<LagCompensator>().AsSingle();
            Container.BindInterfacesTo<RaycastShotResolver>().AsSingle().NonLazy();
            MatchSessionInstaller.Install(Container, _match);
        }

        private void BindClient()
        {
            BindPhysics();
            Container.BindInterfacesTo<PredictableCollection>().AsSingle();
            Container.BindInterfacesTo<PhysicsRollback>().AsSingle().WithArguments(gameObject.Lifetime()).NonLazy();
            Container.BindInterfacesTo<MovingPlatformCollection>().AsSingle();
            Container.BindInterfacesTo<ShovableCollection>().AsSingle();
            Container.BindInterfacesTo<ScheduledCollection>().AsSingle();
            Container.BindInterfacesTo<PredictedBodyCollection>().AsSingle();
            Container.Bind(typeof(IReadonlyFloorWeapons), typeof(IFloorWeapons)).To<FloorWeapons>().AsSingle()
                .WriterOnlyInto<IFloorWeapons>(typeof(FloorWeaponRegistration));
        }

        private void BindPhysics()
        {
            Container.BindInterfacesTo<SweptBodyCollection>().AsSingle();
            Container.Bind(typeof(IPhysicsSpace), typeof(PhysicsSpace)).To<PhysicsSpace>()
                .FromMethod(context => PhysicsSpace.Create(
                    gameObject.Lifetime(),
                    context.Container.Resolve<PhysicsConfig>(),
                    context.Container.Resolve<ISimulationStep>(),
                    context.Container.Resolve<ISweptBodyCollection>()))
                .AsSingle();
            Container.Bind<LevelGeometry>()
                .FromMethod(context => LevelGeometry.Create(gameObject.Lifetime(), context.Container.Resolve<IPhysicsSpace>(), LevelColliders()))
                .AsSingle()
                .NonLazy();
        }

        private Collider2D[] LevelColliders() =>
            Array.FindAll(FindObjectsByType<Collider2D>(FindObjectsInactive.Exclude, FindObjectsSortMode.None), collider => collider.gameObject.scene == gameObject.scene);
    }
}