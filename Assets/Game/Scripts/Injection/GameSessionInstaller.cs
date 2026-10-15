using System;
using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Ticks;
using Bw.Injection.Match;
using Bw.UseCases.Character;
using Bw.UseCases.Match;
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
            Container.BindInterfacesTo<GameObjectByCharacterCollection>().AsSingle();
            Container.BindInterfacesTo<MovingPlatformCollection>().AsSingle();
            Container.BindInterfacesTo<ShovableCollection>().AsSingle();
            Container.BindInterfacesTo<ScheduledCollection>().AsSingle();
            Container.Bind(typeof(IReadonlyWeaponKindRegistry), typeof(IWeaponKindRegistry)).To<WeaponKindRegistry>().AsSingle()
                .WriterOnlyInto<IWeaponKindRegistry>(typeof(WeaponKindRegistration));
            Container.Bind(typeof(IReadonlyWeaponDescriptionRegistry), typeof(IWeaponDescriptionRegistry)).To<WeaponDescriptionRegistry>().AsSingle()
                .WriterOnlyInto<IWeaponDescriptionRegistry>(typeof(WeaponDescriptionRegistration));
            Container.BindInterfacesTo<WeaponInventories>().AsSingle();
            Container.BindInterfacesTo<RewindableCollection>().AsSingle();
            Container.BindInterfacesTo<LagCompensator>().AsSingle();
            Container.BindInterfacesTo<RaycastShotResolver>().AsSingle().NonLazy();
            MatchSessionInstaller.Install(Container, _match);
        }

        private void BindClient()
        {
            Container.BindInterfacesTo<MovingPlatformCollection>().AsSingle();
            Container.BindInterfacesTo<ShovableCollection>().AsSingle();
            Container.BindInterfacesTo<ScheduledCollection>().AsSingle();
            Container.BindInterfacesTo<PredictedBodyCollection>().AsSingle();
            Container.Bind(typeof(IReadonlyFloorWeapons), typeof(IFloorWeapons)).To<FloorWeapons>().AsSingle()
                .WriterOnlyInto<IFloorWeapons>(typeof(FloorWeaponRegistration));
        }
    }
}