using Bw.UseCases.Spawning.Network;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Injection
{
    public class PrefabHandlerInitializer //TODO: переделать
    {
        public PrefabHandlerInitializer(NetworkCharactersPrefabHandler prefabHandler, GameObject networkCharacterPrefab)
        {
            NetworkManager.Singleton.PrefabHandler.AddHandler(networkCharacterPrefab, prefabHandler);
        }
    }
}