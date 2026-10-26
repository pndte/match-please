using Bw.Entities.Infrastructure;
using Bw.Entities.Network;
using JetBrains.Diagnostics;
using Setup.Launch;
using UnityEngine.SceneManagement;
using Zenject;

namespace Bw.Injection
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Log.DefaultFactory = new UnityLogFactory();

            if (SceneManager.GetActiveScene().name != "GameSetupScene")
                SceneManager.LoadScene("GameSetupScene", LoadSceneMode.Single);

            Container.BindInterfacesTo<NetworkHolder>().AsSingle();
            BindLaunch();
        }

        private void BindLaunch()
        {
            Container.Bind<NextLaunch>().AsSingle();
            Container.Bind<SessionScenes>().AsSingle();
            Container.Bind<GameRestarter>().AsSingle();
        }
    }
}
