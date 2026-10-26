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
        private const string BootScene = "Boot";

        public override void InstallBindings()
        {
            Log.DefaultFactory = new UnityLogFactory();

            if (SceneManager.GetActiveScene().name != SetupSceneLoader.SetupScene)
                SceneManager.LoadScene(BootScene, LoadSceneMode.Single);

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
