using Bw.UseCases;
using Setup;
using Zenject;

namespace Bw.Injection
{
    public class ProjectInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<DiContainer>().FromInstance(Container).AsSingle();

            if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.BindInterfacesTo<CharacterRegistry>().AsSingle();
            }
        }
    }
}

