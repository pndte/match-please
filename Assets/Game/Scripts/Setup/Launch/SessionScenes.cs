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

        public UniTask LoadServer(RoomSettings room)
        {
            var settings = new RuntimeSettings(PeerType.Server, room.Level);
            return Load(room.Level, container =>
            {
                container.Bind<IRuntimeSettings>().FromInstance(settings).AsSingle();
                container.BindInstance(room).AsSingle();
            });
        }

        public UniTask LoadClient(string level)
        {
            var settings = new RuntimeSettings(PeerType.Client, level);
            return Load(level, container => container.Bind<IRuntimeSettings>().FromInstance(settings).AsSingle());
        }

        public async UniTask Unload()
        {
            for (var index = _loaded.Count - 1; index >= 0; index--)
                await Addressables.UnloadSceneAsync(_loaded[index]).ToUniTask();

            _loaded.Clear();
        }

        private async UniTask Load(string level, Action<DiContainer> bindSession)
        {
            if (_loaded.Count > 0)
                throw new InvalidOperationException("A session is loaded already: unload it before loading the next one.");

            SceneContext.ExtraBindingsInstallMethod = bindSession;
            await LoadAdditive(NetworkScene);
            var game = await LoadAdditive(level);
            SceneManager.SetActiveScene(game.Scene);
        }

        private async UniTask<SceneInstance> LoadAdditive(string address)
        {
            var handle = Addressables.LoadSceneAsync(address, LoadSceneMode.Additive);
            _loaded.Add(handle);
            return await handle.ToUniTask();
        }
    }
}
