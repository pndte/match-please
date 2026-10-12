using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Setup.Launch
{
    public sealed class StartMenu : MonoBehaviour
    {
        private const string AddressKey = "Launch.ServerAddress";
        private const float Width = 620f;
        private const float Height = 470f;

        private readonly UniTaskCompletionSource<LaunchChoice> _choice = new();

        private ushort _port;
        private string _address;
        private string _localAddresses;
        private string _error = string.Empty;
        private LaunchStyles _styles;

        public UniTask<LaunchChoice> Choose(ushort port, string defaultAddress)
        {
            _port = port;
            _address = PlayerPrefs.GetString(AddressKey, defaultAddress);
            _localAddresses = LocalAddresses.Describe();
            return _choice.Task;
        }

        private void OnGUI()
        {
            _styles ??= new LaunchStyles();
            var scale = Screen.height / 1080f;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            var width = Screen.width / scale;
            var height = Screen.height / scale;

            GUILayout.BeginArea(new Rect((width - Width) / 2f, (height - Height) / 2f, Width, Height), GUI.skin.box);
            GUILayout.Label("Match, Please!", _styles.Title);
            GUILayout.Space(16f);
            GUILayout.Label("Хостите игру? Запустите сервер, а во втором окне игры подключитесь к 127.0.0.1", _styles.Text);
            if (GUILayout.Button($"Запустить сервер (порт {_port})", _styles.Button))
                Choose(new LaunchChoice.Server(_port));

            GUILayout.Label($"Ваш адрес для друзей: {_localAddresses}", _styles.Hint);
            GUILayout.Space(24f);
            GUILayout.Label("Подключиться к серверу", _styles.Text);
            _address = GUILayout.TextField(_address, 64, _styles.Field);
            if (GUILayout.Button("Подключиться", _styles.Button) || EnterPressed())
                Connect();

            GUILayout.Label(_error, _styles.Error);
            GUILayout.EndArea();
        }

        private void Connect()
        {
            if (!LaunchChoice.Client.TryParse(_address, _port, out var client))
            {
                _error = $"Нужен адрес вида 26.12.34.56 или 26.12.34.56:{_port}";
                return;
            }

            PlayerPrefs.SetString(AddressKey, _address.Trim());
            PlayerPrefs.Save();
            Choose(client);
        }

        private void Choose(LaunchChoice choice)
        {
            enabled = false;
            _choice.TrySetResult(choice);
        }

        private static bool EnterPressed()
        {
            var current = Event.current;
            return current.type == EventType.KeyDown && (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter);
        }
    }
}
