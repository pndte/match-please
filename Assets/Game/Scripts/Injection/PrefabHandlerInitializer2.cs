using Bw.UseCases.Spawning.Network;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public class PrefabHandlerInitializer2 : IInitializable //TODO: переделать
    {
        private readonly NetworkWeaponPrefabHandler _networkWeaponPrefabHandler;
        private readonly GameObject _networkWeaponPrefab;

        public PrefabHandlerInitializer2(
            NetworkWeaponPrefabHandler networkWeaponPrefabHandler,
            GameObject networkWeaponPrefab)
        {
            _networkWeaponPrefabHandler = networkWeaponPrefabHandler;
            _networkWeaponPrefab = networkWeaponPrefab;
        }

        public void Initialize()
        {
            NetworkManager.Singleton.PrefabHandler.AddHandler(_networkWeaponPrefab, _networkWeaponPrefabHandler);
        }
    }
}