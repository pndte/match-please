using Bw.Entities.Infrastructure;
using Bw.Entities.Network;
using Bw.UseCases;
using JetBrains.Diagnostics;
using UnityEngine.SceneManagement;
using Zenject;

namespace Bw.Injection
{
    public class ProjectInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        public override void InstallBindings()
        {
            Log.DefaultFactory = new UnityLogFactory();

            if (SceneManager.GetActiveScene().name != "GameSetupScene") 
                SceneManager.LoadScene("GameSetupScene", LoadSceneMode.Single);

            Container.BindInterfacesTo<NetworkHolder>().AsSingle();
            if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.BindInterfacesTo<CharacterRegistry>().AsSingle();
            }
        }
    }
}