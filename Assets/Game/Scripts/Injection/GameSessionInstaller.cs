using System;
using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Character;
using Bw.UseCases.Movement.Physics;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Network;
using Bw.UseCases.Shooting.Weapon.Pickup;
using Bw.UseCases.Shooting.Weapon.Pickup.Abstractions;
using Zenject;

namespace Bw.Injection
{
    public class GameSessionInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

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
            Container.BindInterfacesTo<ScheduledCollection>().AsSingle();
            Container.BindInterfacesTo<WeaponInventories>().AsSingle();
            Container.BindInterfacesTo<RewindableCollection>().AsSingle();
            Container.BindInterfacesTo<LagCompensator>().AsSingle();
            Container.BindInterfacesTo<RaycastShotResolver>().AsSingle().NonLazy();
        }

        private void BindClient()
        {
            Container.BindInterfacesTo<MovingPlatformCollection>().AsSingle();
            Container.BindInterfacesTo<ScheduledCollection>().AsSingle();
            Container.Bind(typeof(IReadonlyFloorWeapons), typeof(IFloorWeapons)).To<FloorWeapons>().AsSingle()
                .WriterOnlyInto<IFloorWeapons>(typeof(FloorWeaponRegistration));
        }
    }
}