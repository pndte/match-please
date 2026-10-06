using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.UseCases.Character;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Network;
using Zenject;

namespace Bw.Injection
{
    public class GameSessionInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        public override void InstallBindings()
        {
            if (_runtimeSettings.CurrentPeerType != PeerType.Server)
                return;
            
            Container.BindInterfacesTo<GameObjectByCharacterCollection>().AsSingle();
            Container.BindInterfacesTo<HeldWeaponCollection>().AsSingle();
            Container.BindInterfacesTo<RewindableCollection>().AsSingle();
            Container.BindInterfacesTo<LagCompensator>().AsSingle();
            Container.BindInterfacesTo<RaycastShotResolver>().AsSingle().NonLazy();
            Container.Bind<DeadCharactersDestroyer>().AsSingle().NonLazy();
        }
    }
}