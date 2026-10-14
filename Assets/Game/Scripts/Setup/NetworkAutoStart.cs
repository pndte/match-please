using System;
using System.Linq;
using Bw.Entities.Network;
using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;
using Setup.Launch;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Setup
{
    public class NetworkAutoStart : MonoBehaviour
    {
        [Header("Auto-Start Configuration")]
        [SerializeField] private bool enableAutoStart = true;
        [SerializeField] private string serverTag = "server";
        [SerializeField] private string clientTag = "client";

        [Header("Debug")]
        [SerializeField] private bool enableLogging = true;

        [Header("Game Scene")]
        [SerializeField] private string _gameScene = "SampleScene";

        private IRuntimeSettings _runtimeSettings;
        private ConnectionConfig _connection;
        private StartMenu _menu;
        private NetworkLaunch _network;

        [Inject]
        private void Construct(
            IRuntimeSettings runtimeSettings,
            ConnectionConfig connection,
            StartMenu menu,
            NetworkLaunch network)
        {
            _runtimeSettings = runtimeSettings;
            _connection = connection;
            _menu = menu;
            _network = network;
        }

        private void Start()
        {
            if (!enableAutoStart)
            {
                Log("Auto-start is disabled.");
                return;
            }

            if (NetworkManager.Singleton == null)
                throw new InvalidOperationException("NetworkManager.Singleton is null: the setup scene must hold the network prefab.");

            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
            StartGame().Forget();
        }

        private async UniTaskVoid StartGame()
        {
            if (LaunchArguments.TryReadOwner(Environment.GetCommandLineArgs(), out var ownerId))
                OwnerProcess.QuitWhenGone(ownerId).Forget();

            var choice = TryChooseWithoutMenu(out var preset) ? preset : await ChooseInMenu();
            _network.ShowStarting(choice);
            _runtimeSettings.Initialize(choice.Switch(_ => PeerType.Server, _ => PeerType.Client, _ => PeerType.Client));
            await LoadServicesAndGame();
            await _network.Start(choice);
            await SceneManager.UnloadSceneAsync("GameSetupScene").ToUniTask();
        }

        private async UniTask<LaunchChoice> ChooseInMenu()
        {
            using var choosing = new LifetimeDefinition();
            return await _menu.Choose(choosing.Lifetime);
        }

        private bool TryChooseWithoutMenu(out LaunchChoice choice) =>
            TryReadPlayModeTag(out choice) || LaunchArguments.TryRead(Environment.GetCommandLineArgs(), _connection.Port, out choice);

        private bool TryReadPlayModeTag(out LaunchChoice choice)
        {
            choice = null;
#if UNITY_EDITOR
            var currentTag = Unity.Multiplayer.PlayMode.CurrentPlayer.Tags.FirstOrDefault();
            if (string.Equals(currentTag, serverTag, StringComparison.OrdinalIgnoreCase))
            {
                Log($"Multiplayer Play Mode tag '{currentTag}': starting as the server.");
                choice = new LaunchChoice.Server(_connection.Port);
                return true;
            }

            if (string.Equals(currentTag, clientTag, StringComparison.OrdinalIgnoreCase))
            {
                if (!LaunchChoice.Client.TryParse(_connection.ServerAddress, _connection.Port, out var client))
                    throw new InvalidOperationException($"The connection config's server address '{_connection.ServerAddress}' is no IPv4 address.");

                Log($"Multiplayer Play Mode tag '{currentTag}': starting as a client.");
                choice = client;
                return true;
            }

#endif
            return false;
        }

        private async UniTask LoadServicesAndGame()
        {
            await SceneManager.LoadSceneAsync("Network", LoadSceneMode.Additive).ToUniTask(); //TODO: addresables for scene loading
            await SceneManager.LoadSceneAsync(_gameScene, LoadSceneMode.Additive).ToUniTask();

            var gameScene = SceneManager.GetSceneByName(_gameScene);
            SceneManager.SetActiveScene(gameScene);
        }

        private void Log(string message)
        {
            if (enableLogging)
                Debug.Log($"[NetworkAutoStart] {message}");
        }
    }
}
