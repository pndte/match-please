using Bw.UseCases.Character;
using Setup;
using Zenject;

namespace Bw.Injection
{
    public class GameSessionInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GameObjectByCharacterCollection>().AsSingle(); //TODO только на сервере
            if (_runtimeSettings.CurrentPeerType != PeerType.Server)
                return;
            
            Container.Bind<DeadCharactersDestroyer>().AsSingle().NonLazy();
        }
    }
}