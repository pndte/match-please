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
        [Tooltip("The player character prefab to spawn (must have NetworkObject component)")]
        [SerializeField]
        private NetworkObject _character;

        [SerializeField] private NetworkObject _weapon;

        [Tooltip("Spawn points for players. If empty, spawns at origin.")] [SerializeField]
        private Transform[] _spawnPoints;

        [Tooltip("Random offset range for spawn positions")] [SerializeField]
        private float _spawnRandomOffset = 2f;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NetworkClientCollection>().AsSingle().WithArguments(NetworkManager.Singleton).NonLazy(); //TODO: должно быть в ProjectContext
            Container.BindInterfacesTo<UniversalPlayerCollection>().AsSingle(); //TODO: должно быть в ProjectContext

            Container.BindInterfacesAndSelfTo<NetworkCharactersPrefabHandler>().AsSingle().WithArguments(_character.gameObject);
            Container.BindInterfacesAndSelfTo<NetworkWeaponPrefabHandler>().AsSingle().WithArguments(_weapon.gameObject);
            Container.BindInterfacesTo<PrefabHandlerInitializer>().AsSingle().WithArguments(_character.gameObject).NonLazy();
            Container.BindInterfacesTo<PrefabHandlerInitializer2>().AsSingle().WithArguments(_weapon.gameObject).NonLazy();

            if (_runtimeSettings.CurrentPeerType != PeerType.Server) return;
            
            Container.BindInterfacesTo<NetworkCharactersSpawner>().AsSingle().WithArguments(
                new NetworkCharactersSpawner.Data
                {
                    SpawnPoints = _spawnPoints, SpawnRandomOffset = _spawnRandomOffset, 
                    Character = _character, Weapon = _weapon
                });
            Container.Bind<CharacterRespawner>().AsSingle().NonLazy();
        }
    }
}