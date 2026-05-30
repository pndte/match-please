using Bw.Entities.Network;
using Bw.UseCases.Clients.Network;
using Bw.UseCases.Players;
using Bw.UseCases.Spawning;
using Bw.UseCases.Spawning.Network;
using Setup;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public class SpawnerInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [Header("Spawn Configuration")]
        [SerializeField] private NetworkObject _character;
        [SerializeField] private NetworkObject _weapon;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private float _spawnRandomOffset = 2f;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NetworkClientCollection>().AsSingle()
                .WithArguments(NetworkManager.Singleton).NonLazy();
            Container.BindInterfacesTo<UniversalPlayerCollection>().AsSingle();

            if (_runtimeSettings.CurrentPeerType != PeerType.Server)
                return;

            Container.BindInterfacesTo<NetworkCharactersSpawner>().AsSingle().WithArguments(
                new NetworkCharactersSpawner.Data
                {
                    SpawnPoints = _spawnPoints,
                    SpawnRandomOffset = _spawnRandomOffset,
                    CharacterPrefab = _character,
                    WeaponPrefab = _weapon,
                });
            Container.Bind<CharacterRespawner>().AsSingle().NonLazy();
        }
    }
}
