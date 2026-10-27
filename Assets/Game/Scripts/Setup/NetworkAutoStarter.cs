using System;
using System.Linq;
using Bw.Entities.Network;
using Bw.UseCases.Bots;
using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;
using Setup.Launch;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Setup
{
    public class NetworkAutoStarter : MonoBehaviour
    {
        [Header("Auto-Start Configuration")]
        [SerializeField] private bool enableAutoStart = true;
        [SerializeField] private string serverTag = "server";
        [SerializeField] private string clientTag = "client";
        [SerializeField] private string pingTagPrefix = "ping";

        [Header("Debug")]
        [SerializeField] private bool enableLogging = true;

        [Header("Game Scene")]
        [SerializeField] private string _gameScene = "SampleScene";

        private ConnectionConfig _connection;
        private StartMenu _menu;
        private NetworkLauncher _launcher;
        private NextLaunch _next;
        private SessionScenes _scenes;
        private RoomBotsConfig _roomBots;

        [Inject]
        private void Construct(
            ConnectionConfig connection,
            StartMenu menu,
            NetworkLauncher launcher,
            NextLaunch next,
            SessionScenes scenes,
            RoomBotsConfig roomBots)
        {
            _connection = connection;
            _menu = menu;
            _launcher = launcher;
            _next = next;
            _scenes = scenes;
            _roomBots = roomBots;
        }

        private void Start()
        {
            if (!enableAutoStart)
            {
                Log("Auto-start is disabled.");
                return;
            }

            if (NetworkManager.Singleton == null)
                throw new InvalidOperationException("NetworkManager.Singleton is null: the setup scene must hold the launcher prefab.");

            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
            StartGame().Forget();
        }

        private async UniTaskVoid StartGame()
        {
            var choice = await _next.Switch(ChooseAtProcessStart, ChooseInMenu, UniTask.FromResult);
            _launcher.ShowStarting(choice);
            if (!await TryLoadSession(choice))
                return;

            SimulatePlayModePing();
            await _launcher.Start(choice);
        }

        private UniTask<bool> TryLoadSession(LaunchChoice choice) =>
            choice.Switch(
                server => Loaded(_scenes.LoadServer(server.Settings)),
                TryLoadServerLevel,
                room => Loaded(_scenes.LoadClient(room.Settings.Level)));

        private async UniTask<bool> TryLoadServerLevel(LaunchChoice.Client server)
        {
            var answer = await _launcher.AskLevel(server);
            return await answer.Switch(level => Loaded(_scenes.LoadClient(level)), () => UniTask.FromResult(false));
        }

        private static async UniTask<bool> Loaded(UniTask loading)
        {
            await loading;
            return true;
        }

        private RoomSettings DefaultRoom()
        {
            var bots = new RoomBot[_roomBots.StartingBots.Length];
            for (var index = 0; index < bots.Length; index++)
            {
                var bot = _roomBots.StartingBots[index];
                bots[index] = new RoomBot(bot.Nickname, bot.Difficulty.Key, bot.Personality.Key);
            }

            return new RoomSettings(_gameScene, Guid.NewGuid(), bots);
        }

        private UniTask<LaunchChoice> ChooseAtProcessStart()
        {
            if (LaunchArguments.TryReadOwner(Environment.GetCommandLineArgs(), out var ownerId))
                OwnerProcess.QuitWhenGone(ownerId).Forget();

            return TryChooseWithoutMenu(out var preset) ? UniTask.FromResult(preset) : ChooseInMenu();
        }

        private async UniTask<LaunchChoice> ChooseInMenu()
        {
            using var choosing = new LifetimeDefinition();
            return await _menu.Choose(choosing.Lifetime);
        }

        private bool TryChooseWithoutMenu(out LaunchChoice choice) =>
            TryReadPlayModeTag(out choice) || LaunchArguments.TryRead(Environment.GetCommandLineArgs(), _connection.Port, DefaultRoom(), out choice);

        private bool TryReadPlayModeTag(out LaunchChoice choice)
        {
            choice = null;
#if UNITY_EDITOR
            var currentTag = Unity.Multiplayer.PlayMode.CurrentPlayer.Tags.FirstOrDefault(tag =>
                string.Equals(tag, serverTag, StringComparison.OrdinalIgnoreCase) || string.Equals(tag, clientTag, StringComparison.OrdinalIgnoreCase));
            if (string.Equals(currentTag, serverTag, StringComparison.OrdinalIgnoreCase))
            {
                Log($"Multiplayer Play Mode tag '{currentTag}': starting as the server.");
                choice = new LaunchChoice.Server(_connection.Port, DefaultRoom());
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

        private void SimulatePlayModePing()
        {
#if UNITY_EDITOR
            foreach (var tag in Unity.Multiplayer.PlayMode.CurrentPlayer.Tags)
            {
                if (!tag.StartsWith(pingTagPrefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!int.TryParse(tag.Substring(pingTagPrefix.Length), out var ping) || ping <= 0)
                    throw new InvalidOperationException($"Multiplayer Play Mode tag '{tag}' must name a ping in milliseconds, such as '{pingTagPrefix}150'.");

                var simulator = NetworkManager.Singleton.gameObject.AddComponent<Unity.Multiplayer.Tools.NetworkSimulator.Runtime.NetworkSimulator>();
                simulator.ConnectionPreset = Unity.Multiplayer.Tools.NetworkSimulator.Runtime.NetworkSimulatorPreset.Create($"Ping {ping} ms", packetDelayMs: ping / 2);
                Log($"Multiplayer Play Mode tag '{tag}': simulating a {ping} ms ping, {ping / 2} ms each way.");
                return;
            }
#endif
        }

        private void Log(string message)
        {
            if (enableLogging)
                Debug.Log($"[NetworkAutoStarter] {message}");
        }
    }
}
