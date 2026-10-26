using System;
using JetBrains.Lifetimes;
using Setup.Launch.Menu;
using Unity.Netcode;

namespace Setup.Launch
{
    public sealed class ConnectionMenu
    {
        private readonly Lifetime _lifetime;
        private readonly MenuDocument _document;
        private readonly ConnectingScreen _connecting;
        private readonly ConnectionFailedScreen _failed;
        private readonly SequentialLifetimes _spinning;

        public ConnectionMenu(
            Lifetime lifetime,
            MenuDocument document,
            ConnectingScreen connecting,
            ConnectionFailedScreen failed)
        {
            _lifetime = lifetime;
            _document = document;
            _connecting = connecting;
            _failed = failed;
            _spinning = new SequentialLifetimes(lifetime);

            connecting.CancelChosen.Advise(lifetime, _ => GameRestarter.ReturnToMenu());
            failed.BackChosen.Advise(lifetime, _ => GameRestarter.ReturnToMenu());
        }

        public void ShowConnecting(LaunchChoice.Client server) =>
            _connecting.Show(_spinning.Next(), server.Endpoint());

        public void Watch(NetworkManager manager, LaunchChoice.Client server, LaunchChoice chosen)
        {
            var connected = false;
            _failed.RetryChosen.Advise(_lifetime, _ => GameRestarter.Relaunch(chosen));
            manager.AdviseConnectionEvents(_lifetime, connection =>
            {
                if (connection.EventType != ConnectionEvent.ClientConnected || connection.ClientId != manager.LocalClientId)
                    return;

                connected = true;
                _spinning.TerminateCurrent();
                _document.Hide();
            });
            manager.AdviseClientStopped(_lifetime, () => ShowFailed(connected
                ? $"Связь с сервером {server.Endpoint()} потеряна."
                : NoAnswerOf(server, chosen)));
        }

        public void ShowStartFailed(LaunchChoice.Client server) =>
            ShowFailed($"Не получилось начать подключение к {server.Endpoint()}.");

        private void ShowFailed(string reason)
        {
            _spinning.TerminateCurrent();
            _failed.Show(reason);
        }

        private static string NoAnswerOf(LaunchChoice.Client server, LaunchChoice chosen) =>
            chosen.Switch(
                _ => throw new InvalidOperationException("A server connects to nobody: only a join or an own room is watched."),
                _ => $"Сервер {server.Endpoint()} не отвечает. Проверь адрес и что комната уже создана.",
                room => $"Комната на порту {room.Port} не открылась: сервер в фоне не ответил. Его лог: {BackgroundServer.LogOf(room.Port)}");
    }
}
