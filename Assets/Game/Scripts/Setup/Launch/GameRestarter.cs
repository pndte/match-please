using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Setup.Launch
{
    public sealed class GameRestarter
    {
        private readonly NextLaunch _next;
        private readonly SessionScenes _scenes;

        private bool _restarting;

        public GameRestarter(NextLaunch next, SessionScenes scenes)
        {
            _next = next;
            _scenes = scenes;
        }

        public void ReturnToMenu()
        {
            _next.OpenMenu();
            Restart().Forget();
        }

        public void Relaunch(LaunchChoice choice)
        {
            _next.StartWith(choice);
            Restart().Forget();
        }

        private async UniTaskVoid Restart()
        {
            if (_restarting)
                return;

            _restarting = true;
            try
            {
                await StopNetwork(NetworkManager.Singleton);
                await _scenes.Unload();
                await Addressables.LoadSceneAsync(SetupSceneLoader.SetupScene, LoadSceneMode.Single).ToUniTask();
            }
            finally
            {
                _restarting = false;
            }
        }

        private static async UniTask StopNetwork(NetworkManager manager)
        {
            if (manager.IsListening)
            {
                manager.Shutdown();
                await UniTask.WaitWhile(() => manager.ShutdownInProgress);
            }

            Object.Destroy(manager.gameObject);
        }
    }
}
