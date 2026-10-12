using System;
using System.Diagnostics;
using Unity.Netcode;
using UnityEngine;

namespace Setup.Launch
{
    public sealed class NetworkStatusOverlay : MonoBehaviour
    {
        private const float Width = 640f;
        private const float Height = 320f;

        private NetworkManager _manager;
        private LaunchChoice _choice;
        private string _localAddresses;
        private LaunchStyles _styles;
        private float _scale;

        public static void Show(NetworkManager manager, LaunchChoice choice)
        {
            var overlay = new GameObject(nameof(NetworkStatusOverlay)).AddComponent<NetworkStatusOverlay>();
            DontDestroyOnLoad(overlay.gameObject);
            overlay._manager = manager;
            overlay._choice = choice;
            overlay._localAddresses = LocalAddresses.Describe();
        }

        private void OnGUI()
        {
            _styles ??= new LaunchStyles();
            _scale = Screen.height / 1080f;
            GUI.matrix = Matrix4x4.Scale(new Vector3(_scale, _scale, 1f));
            _choice.Switch(DrawServer, DrawClient);
        }

        private void DrawServer(LaunchChoice.Server server)
        {
            if (_manager.IsServer && _manager.IsListening)
            {
                GUILayout.BeginArea(new Rect(16f, 16f, Width, 120f), GUI.skin.box);
                GUILayout.Label($"Сервер работает · порт {server.Port} · игроков: {_manager.ConnectedClientsIds.Count}", _styles.Text);
                GUILayout.Label($"Адрес для друзей: {_localAddresses}", _styles.Hint);
                GUILayout.EndArea();
                return;
            }

            BeginPanel();
            GUILayout.Label($"Сервер не запустился на порту {server.Port}: возможно, его уже заняла другая копия игры", _styles.Text);
            QuitButton();
            GUILayout.EndArea();
        }

        private void DrawClient(LaunchChoice.Client client)
        {
            if (_manager.IsConnectedClient)
                return;

            var endpoint = $"{client.Address}:{client.Port}";
            BeginPanel();
            if (_manager.IsListening)
            {
                GUILayout.Label($"Подключение к {endpoint}…", _styles.Text);
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label($"Нет связи с сервером {endpoint}", _styles.Text);
            GUILayout.Label(_manager.DisconnectReason ?? string.Empty, _styles.Hint);
#if !UNITY_EDITOR
            if (GUILayout.Button("Переподключиться", _styles.Button))
                Relaunch($"{LaunchArguments.Connect} {endpoint}");

            if (GUILayout.Button("В меню", _styles.Button))
                Relaunch(string.Empty);

#endif
            QuitButton();
            GUILayout.EndArea();
        }

        private void BeginPanel()
        {
            var width = Screen.width / _scale;
            var height = Screen.height / _scale;
            GUILayout.BeginArea(new Rect((width - Width) / 2f, (height - Height) / 2f, Width, Height), GUI.skin.box);
        }

        private void QuitButton()
        {
            if (GUILayout.Button("Выйти", _styles.Button))
                Application.Quit();
        }

        private static void Relaunch(string arguments)
        {
            Process.Start(Environment.GetCommandLineArgs()[0], arguments);
            Application.Quit();
        }
    }
}
