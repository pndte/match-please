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
        private readonly GameRestarter _restarter;
        private readonly SequentialLifetimes _spinning;

        public ConnectionMenu(
            Lifetime lifetime,
            MenuDocument document,
            ConnectingScreen connecting,
            ConnectionFailedScreen failed,
            GameRestarter restarter)
        {
            _lifetime = lifetime;
            _document = document;
            _connecting = connecting;
            _failed = failed;
            _restarter = restarter;
            _spinning = new SequentialLifetimes(lifetime);

            connecting.CancelChosen.Advise(lifetime, _ => restarter.ReturnToMenu());
            failed.BackChosen.Advise(lifetime, _ => restarter.ReturnToMenu());
        }

        public void ShowConnecting(LaunchChoice.Client server) =>
            _connecting.Show(_spinning.Next(), server.Endpoint());

        public void Watch(NetworkManager manager, LaunchChoice.Client server, LaunchChoice chosen)
        {
            var connected = false;
            _failed.RetryChosen.Advise(_lifetime, _ => _restarter.Relaunch(chosen));
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

        public void ShowUnanswered(LaunchChoice.Client server)
        {
            _failed.RetryChosen.Advise(_lifetime, _ => _restarter.Relaunch(server));
            ShowFailed($"Сервер {server.Endpoint()} не сказал, какая у него карта. Проверь адрес, что комната создана и что у вас одна версия игры.");
        }

        public void ShowStartFailed(LaunchChoice.Client server) =>
            ShowFailed($"Не получилось начать подключение к {server.Endpoint()}.");

        public void ShowRoomServerMissing() =>
            ShowFailed($"Комнату открывает сервер из сборки игры, а её нет: {BackgroundServer.Executable}. Собери её через Tools → Build → Windows Client.");

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
