using System.Linq;
using Bw.Entities.Network;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

#if UNITY_EDITOR

#endif

namespace Setup
{
    /// <summary>
    /// Automatically starts NetworkManager based on Multiplayer Play Mode tags.
    /// Supports "server" and "client" tags for automatic server/client startup.
    /// Also initializes the RuntimeSettings with the detected peer type.
    /// </summary>
    public class NetworkAutoStart : MonoBehaviour
    {
        [Header("Auto-Start Configuration")]
        [Tooltip("Enable automatic startup based on Multiplayer Play Mode tags")]
        [SerializeField]
        private bool enableAutoStart = true;

        [Tooltip("Tag name for server instances (default: 'server')")] [SerializeField]
        private string serverTag = "server";

        [Tooltip("Tag name for client instances (default: 'client')")] [SerializeField]
        private string clientTag = "client";

        [Header("Connection Settings")]
        [Tooltip("IP address for client to connect to (default: 127.0.0.1)")]
        [SerializeField]
        private string serverAddress = "127.0.0.1";

        [Tooltip("Port for network communication (default: 7777)")] [SerializeField]
        private ushort port = 7777;

        [Header("Debug")] [Tooltip("Enable detailed logging")] [SerializeField]
        private bool enableLogging = true;

        // Injected dependency
        private IRuntimeSettings _runtimeSettings;
        private INetworkHolder _networkHolder;

        [Inject]
        private void Construct(IRuntimeSettings runtimeSettings, INetworkHolder networkHolder)
        {
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
            {
                Debug.LogError(
                    "[NetworkAutoStart] NetworkManager.Singleton is null! Make sure NetworkManager exists in the scene.");
                return;
            }

            Log("Auto-start startoing.");

            // Detect and handle Multiplayer Play Mode tag
            DetectAndStartNetwork();
            _networkHolder.NetworkManager.Value = NetworkManager.Singleton;

            LoadServicesAndGame().Forget();
        }

        private async UniTaskVoid LoadServicesAndGame()
        {
            await SceneManager.LoadSceneAsync("Network", LoadSceneMode.Additive).ToUniTask(); //TODO: addresables for scene loading
            await SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Additive).ToUniTask();
            
            var gameScene = SceneManager.GetSceneByName("SampleScene");
            SceneManager.SetActiveScene(gameScene);
            
            await SceneManager.UnloadSceneAsync("GameSetupScene").ToUniTask();


            // var gameSetupScene = SceneManager.GetSceneByName("GameSetupScene");
            // await SceneManager.UnloadSceneAsync(gameSetupScene);
        }

        private void DetectAndStartNetwork()
        {
#if UNITY_EDITOR
            // Get the current player's tag from Multiplayer Play Mode
            string currentTag = Unity.Multiplayer.PlayMode.CurrentPlayer.Tags.First();

            if (string.IsNullOrEmpty(currentTag))
            {
                Log("No Multiplayer Play Mode tag detected. Running in normal Editor mode or standalone build.");
                return;
            }

            Log($"Detected Multiplayer Play Mode tag: '{currentTag}'");

            // Start NetworkManager based on the detected tag
            if (currentTag.Equals(serverTag, System.StringComparison.OrdinalIgnoreCase))
            {
                StartAsServer();
            }
            else if (currentTag.Equals(clientTag, System.StringComparison.OrdinalIgnoreCase))
            {
                StartAsClient();
            }
            else
            {
                Log(
                    $"Unknown tag '{currentTag}'. Expected '{serverTag}' or '{clientTag}'. No automatic startup performed.");
            }
#else
            Log("Multiplayer Play Mode is only available in the Unity Editor. Running in standalone build mode.");
#endif
        }

        private void StartAsServer()
        {
            Log($"<color=green>Starting as SERVER on port {port}...</color>");

            // Configure transport if needed
            ConfigureTransport();

            bool success = NetworkManager.Singleton.StartServer();

            if (success)
            {
                Log($"<color=green>✓ Server started successfully on port {port}</color>");

                // Initialize RuntimeSettings with Server peer type
                _runtimeSettings.Initialize(PeerType.Server);
            }
            else
            {
                Debug.LogError("[NetworkAutoStart] Failed to start server!");
            }
        }

        private void StartAsClient()
        {
            Log($"<color=cyan>Starting as CLIENT connecting to {serverAddress}:{port}...</color>");

            // Configure transport if needed
            ConfigureTransport();

            bool success = NetworkManager.Singleton.StartClient();

            if (success)
            {
                Log($"<color=cyan>✓ Client started successfully, connecting to {serverAddress}:{port}</color>");

                // Initialize RuntimeSettings with Client peer type
                _runtimeSettings.Initialize(PeerType.Client);
            }
            else
            {
                Debug.LogError("[NetworkAutoStart] Failed to start client!");
            }
        }

        private void ConfigureTransport()
        {
            // Try to configure UnityTransport if it exists
            var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;

            if (transport != null)
            {
                // Use reflection to set connection data on UnityTransport
                var transportType = transport.GetType();

                // Check if it's UnityTransport
                if (transportType.Name == "UnityTransport")
                {
                    try
                    {
                        // Set connection data
                        var connectionDataField = transportType.GetField("ConnectionData");
                        if (connectionDataField != null)
                        {
                            var connectionData = connectionDataField.GetValue(transport);
                            var connectionDataType = connectionData.GetType();

                            // Set Address
                            var addressField = connectionDataType.GetField("Address");
                            if (addressField != null)
                            {
                                addressField.SetValue(connectionData, serverAddress);
                            }

                            // Set Port
                            var portField = connectionDataType.GetField("Port");
                            if (portField != null)
                            {
                                portField.SetValue(connectionData, port);
                            }

                            connectionDataField.SetValue(transport, connectionData);
                            Log($"Transport configured: {serverAddress}:{port}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning(
                            $"[NetworkAutoStart] Could not configure transport via reflection: {ex.Message}");
                    }
                }
            }
        }

        private void Log(string message)
        {
            if (enableLogging)
            {
                Debug.Log($"[NetworkAutoStart] {message}");
            }
        }

        #region Debug GUI (Optional)

        [Header("GUI")] [SerializeField] private bool showDebugGUI = true;

        private void OnGUI()
        {
            if (!showDebugGUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 400, 250));
            GUILayout.Label("=== Network Auto-Start ===");

#if UNITY_EDITOR
            string currentTag = Unity.Multiplayer.PlayMode.CurrentPlayer.Tags.First();
            GUILayout.Label($"Multiplayer Play Mode Tag: {(string.IsNullOrEmpty(currentTag) ? "None" : currentTag)}");
#else
            GUILayout.Label("Multiplayer Play Mode: Not available (Standalone build)");
#endif

            // Display RuntimeSettings status
            if (_runtimeSettings != null)
            {
                GUILayout.Label($"Runtime Peer Type: <color=yellow>{_runtimeSettings.CurrentPeerType}</color>");
            }

            if (NetworkManager.Singleton != null)
            {
                GUILayout.Label($"Network Status: {GetNetworkStatus()}");

                if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
                {
                    GUILayout.Label("Manual Controls:");

                    if (GUILayout.Button("Start Server", GUILayout.Height(30)))
                    {
                        StartAsServer();
                    }

                    if (GUILayout.Button("Start Client", GUILayout.Height(30)))
                    {
                        StartAsClient();
                    }

                    if (GUILayout.Button("Start Host", GUILayout.Height(30)))
                    {
                        Log("<color=yellow>Starting as HOST...</color>");
                        NetworkManager.Singleton.StartHost();
                    }
                }
                else
                {
                    if (GUILayout.Button("Shutdown", GUILayout.Height(30)))
                    {
                        Log("Shutting down network...");
                        NetworkManager.Singleton.Shutdown();
                    }
                }
            }
            else
            {
                GUILayout.Label("<color=red>NetworkManager.Singleton is null!</color>");
            }

            GUILayout.EndArea();
        }

        private string GetNetworkStatus()
        {
            if (NetworkManager.Singleton == null) return "No NetworkManager";

            if (NetworkManager.Singleton.IsHost)
                return "<color=yellow>Host (Server + Client)</color>";
            if (NetworkManager.Singleton.IsServer)
                return "<color=green>Server</color>";
            if (NetworkManager.Singleton.IsClient)
                return "<color=cyan>Client</color>";

            return "Not Started";
        }

        #endregion
    }
}