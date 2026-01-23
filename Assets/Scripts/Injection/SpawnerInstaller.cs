using Bw.UseCases.Character;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Clients.Network;
using Bw.UseCases.Spawning.Network;
using JetBrains.Lifetimes;
using Setup;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public class SpawnerInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [SerializeField] private NetworkCharactersSpawner _networkCharactersSpawner;
        [SerializeField] private NetworkClientCollection _networkClientCollection;
        
        [Header("Spawn Configuration")]
        [Tooltip("The player character prefab to spawn (must have NetworkObject component)")]
        [SerializeField] private NetworkCharacter _character;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NetworkClientCollection>().FromInstance(_networkClientCollection).AsSingle();

            Container.BindInterfacesAndSelfTo<NetworkCharactersPrefabHandler>().AsSingle().WithArguments(_character.gameObject);
            Container.BindInterfacesTo<PrefabHandlerInitializer>().AsSingle().WithArguments(_character.gameObject).NonLazy();
            if (_runtimeSettings.CurrentPeerType != PeerType.Server) return;

            Container.BindInterfacesTo<NetworkCharactersSpawner>().FromInstance(_networkCharactersSpawner).AsSingle();
            Container.Bind<CharacterRespawner>().AsSingle().NonLazy();
        }
    }
}