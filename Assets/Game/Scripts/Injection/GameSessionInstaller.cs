using Bw.Entities.Network;
using Bw.UseCases.Character;
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
            Container.Bind<DeadCharactersDestroyer>().AsSingle().NonLazy();
        }
    }
}