using Bw.UseCases.Spawning.Network;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public class PrefabHandlerInitializer : IInitializable //TODO: переделать
    {
        private readonly NetworkCharactersPrefabHandler _prefabHandler;
        private readonly GameObject _networkCharacterPrefab;

        public PrefabHandlerInitializer(
            NetworkCharactersPrefabHandler prefabHandler,
            GameObject networkCharacterPrefab)
        {
            _prefabHandler = prefabHandler;
            _networkCharacterPrefab = networkCharacterPrefab;
        }

        public void Initialize()
        {
            NetworkManager.Singleton.PrefabHandler.AddHandler(_networkCharacterPrefab, _prefabHandler);

        }
    }
}