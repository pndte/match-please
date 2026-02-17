using Bw.Entities.Network;
using Bw.UseCases;
using Setup;
using UnityEngine.SceneManagement;
using Zenject;

namespace Bw.Injection
{
    public class ProjectInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        public override void InstallBindings()
        {
            if (SceneManager.GetActiveScene().name != "LoadingScene") 
                SceneManager.LoadScene("LoadingScene", LoadSceneMode.Single);
            Container.BindInterfacesAndSelfTo<DiContainer>().FromInstance(Container).AsSingle();

            if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.BindInterfacesTo<CharacterRegistry>().AsSingle();
            }
        }
    }
}