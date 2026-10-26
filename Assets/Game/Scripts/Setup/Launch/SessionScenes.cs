using System;
using System.Collections.Generic;
using Bw.Entities.Network;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Zenject;

namespace Setup.Launch
{
    public sealed class SessionScenes //TODO: сделать аренду сцен через lifetime
    {
        private const string NetworkScene = "Network";

        private readonly List<AsyncOperationHandle<SceneInstance>> _loaded = new();

        public async UniTask Load(PeerType peer, string gameScene)
        {
            if (_loaded.Count > 0)
                throw new InvalidOperationException("A session is loaded already: unload it before loading the next one.");

            var settings = new RuntimeSettings(peer);
            SceneContext.ExtraBindingsInstallMethod = container => container.Bind<IRuntimeSettings>().FromInstance(settings).AsSingle();
            await LoadAdditive(NetworkScene);
            var game = await LoadAdditive(gameScene);
            SceneManager.SetActiveScene(game.Scene);
        }

        public async UniTask Unload()
        {
            for (var index = _loaded.Count - 1; index >= 0; index--)
                await Addressables.UnloadSceneAsync(_loaded[index]).ToUniTask();

            _loaded.Clear();
        }

        private async UniTask<SceneInstance> LoadAdditive(string address)
        {
            var handle = Addressables.LoadSceneAsync(address, LoadSceneMode.Additive);
            _loaded.Add(handle);
            return await handle.ToUniTask();
        }
    }
}
