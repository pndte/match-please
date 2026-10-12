using System;
using System.Linq;
using Bw.Entities.Network;
using Cysharp.Threading.Tasks;
using Setup.Launch;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Setup
{
    public class NetworkAutoStart : MonoBehaviour
    {
        private const string ListenAddress = "0.0.0.0";
        private const int ServerFrameRate = 60;
        private const int ConnectAttempts = 10;
        private const int ServerWindowWidth = 960;
        private const int ServerWindowHeight = 540;

        [Header("Auto-Start Configuration")]
        [SerializeField] private bool enableAutoStart = true;
        [SerializeField] private string serverTag = "server";
        [SerializeField] private string clientTag = "client";

        [Header("Debug")]
        [SerializeField] private bool enableLogging = true;

        [Header("Game Scene")]
        [SerializeField] private string _gameScene = "SampleScene";

        private IRuntimeSettings _runtimeSettings;
        private INetworkHolder _networkHolder;
        private ConnectionConfig _connection;

        [Inject]
        private void Construct(IRuntimeSettings runtimeSettings, INetworkHolder networkHolder, ConnectionConfig connection)
        {
            _connection = connection;
            _networkHolder = networkHolder;
            _runtimeSettings = runtimeSettings;
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

            StartGame().Forget();
        }

        private async UniTaskVoid StartGame()
        {
            var choice = await Choose();
            _runtimeSettings.Initialize(choice.Switch(_ => PeerType.Server, _ => PeerType.Client));
            await LoadServicesAndGame();
            StartNetwork(choice);
            await SceneManager.UnloadSceneAsync("GameSetupScene").ToUniTask();
        }

        private UniTask<LaunchChoice> Choose()
        {
            if (TryReadPlayModeTag(out var tagged))
                return UniTask.FromResult(tagged);

            if (LaunchArguments.TryRead(Environment.GetCommandLineArgs(), _connection.Port, out var argued))
                return UniTask.FromResult(argued);

            return gameObject.AddComponent<StartMenu>().Choose(_connection.Port, _connection.ServerAddress);
        }

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

        private void StartNetwork(LaunchChoice choice)
        {
            var manager = NetworkManager.Singleton;
            var transport = (UnityTransport)manager.NetworkConfig.NetworkTransport;
            var started = choice.Switch(
                server => StartServer(manager, transport, server),
                client => StartClient(manager, transport, client));

            NetworkStatusOverlay.Show(manager, choice);
            if (!started)
            {
                Debug.LogError("[NetworkAutoStart] The network did not start.");
                return;
            }

            _networkHolder.NetworkManager.Value = manager;
        }

        private bool StartServer(NetworkManager manager, UnityTransport transport, LaunchChoice.Server server)
        {
            Log($"Starting as the server on {ListenAddress}:{server.Port}");
            transport.SetConnectionData(true, ListenAddress, server.Port, ListenAddress);
            Application.targetFrameRate = ServerFrameRate;
            Screen.SetResolution(ServerWindowWidth, ServerWindowHeight, FullScreenMode.Windowed);
            return manager.StartServer();
        }

        private bool StartClient(NetworkManager manager, UnityTransport transport, LaunchChoice.Client client)
        {
            Log($"Starting as a client of {client.Address}:{client.Port}");
            transport.SetConnectionData(true, client.Address, client.Port);
            transport.MaxConnectAttempts = ConnectAttempts;
            return manager.StartClient();
        }

        private void Log(string message)
        {
            if (enableLogging)
                Debug.Log($"[NetworkAutoStart] {message}");
        }
    }
}
