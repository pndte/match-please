using System;
using Bw.Entities.Network;
using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Setup.Launch
{
    public sealed class NetworkLauncher
    {
        private const string ListenAddress = "0.0.0.0";
        private const int ServerFrameRate = 60;
        private const int JoinAttempts = 10;
        private const int OwnRoomAttempts = 30;
        private const int ServerWindowWidth = 960;
        private const int ServerWindowHeight = 540;

        private readonly Lifetime _lifetime;
        private readonly INetworkHolder _networkHolder;
        private readonly RoomMenu _room;
        private readonly ConnectionMenu _connection;
        private readonly IPlayerProfile _profile;

        public NetworkLauncher(
            Lifetime lifetime,
            INetworkHolder networkHolder,
            RoomMenu room,
            ConnectionMenu connection,
            IPlayerProfile profile)
        {
            _lifetime = lifetime;
            _networkHolder = networkHolder;
            _room = room;
            _connection = connection;
            _profile = profile;
        }

        public void ShowStarting(LaunchChoice choice) =>
            choice.Switch(
                server => _room.ShowOpening(server.Port),
                client => _connection.ShowConnecting(client),
                room => _connection.ShowConnecting(LaunchChoice.Client.Loopback(room.Port)));

        public async UniTask<LevelAnswer> AskLevel(LaunchChoice.Client server)
        {
            var manager = NetworkManager.Singleton;
            var transport = TransportOf(manager);
            transport.SetConnectionData(true, server.Address, server.Port);
            transport.MaxConnectAttempts = JoinAttempts;
            manager.NetworkConfig.ConnectionData = LevelQuery.Payload;
            using var asking = new LifetimeDefinition();
            var stopped = new UniTaskCompletionSource();
            var admitted = false;
            manager.AdviseClientStopped(asking.Lifetime, () => stopped.TrySetResult());
            manager.AdviseConnectionEvents(asking.Lifetime, connection => admitted |= connection.EventType == ConnectionEvent.ClientConnected);
            if (!manager.StartClient())
                return Unanswered(server);

            await stopped.Task;
            await UniTask.WaitWhile(() => manager.ShutdownInProgress);
            return !admitted && LevelQuery.TryReadAnswer(manager.DisconnectReason, out var level)
                ? new LevelAnswer.Given(level)
                : Unanswered(server);
        }

        public UniTask Start(LaunchChoice choice)
        {
            var manager = NetworkManager.Singleton;
            return choice.Switch(
                server => StartServer(manager, server),
                client => Join(manager, client),
                room => OpenRoom(manager, room));
        }

        private async UniTask StartServer(NetworkManager manager, LaunchChoice.Server server)
        {
            Application.targetFrameRate = ServerFrameRate;
            Screen.SetResolution(ServerWindowWidth, ServerWindowHeight, FullScreenMode.Windowed);
            var port = server.Port;
            while (!TryListen(manager, port))
            {
                if (Application.isBatchMode)
                {
                    Debug.LogError($"[NetworkLauncher] Port {port} is busy, and a batch mode server has no window to pick another one: quitting.");
                    Application.Quit(1);
                    return;
                }

                port = (await RetryRoom(port)).Port;
            }

            _room.ShowOpen(manager, port);
            _networkHolder.NetworkManager.Value = manager;
        }

        private UniTask Join(NetworkManager manager, LaunchChoice.Client server)
        {
            Connect(manager, server, server, Guid.Empty, JoinAttempts);
            return UniTask.CompletedTask;
        }

        private async UniTask OpenRoom(NetworkManager manager, LaunchChoice.Room room)
        {
            if (!BackgroundServer.Available)
            {
                _connection.ShowRoomServerMissing();
                return;
            }

            while (!LocalPorts.IsFree(room.Port))
                room = await RetryRoom(room.Port);

            BackgroundServer.Open(_lifetime, new LaunchChoice.Server(room.Port, room.Settings));
            Connect(manager, LaunchChoice.Client.Loopback(room.Port), room, room.Settings.HostKey, OwnRoomAttempts);
        }

        private async UniTask<LaunchChoice.Room> RetryRoom(ushort busyPort)
        {
            using var retrying = new LifetimeDefinition();
            return await _room.RetryRoom(retrying.Lifetime, busyPort);
        }

        private LevelAnswer Unanswered(LaunchChoice.Client server)
        {
            _connection.ShowUnanswered(server);
            return LevelAnswer.Missing.Instance;
        }

        private static bool TryListen(NetworkManager manager, ushort port)
        {
            Debug.Log($"[NetworkLauncher] Starting as the server on {ListenAddress}:{port}");
            TransportOf(manager).SetConnectionData(true, ListenAddress, port, ListenAddress);
            return manager.StartServer();
        }

        private void Connect(
            NetworkManager manager,
            LaunchChoice.Client server,
            LaunchChoice chosen,
            Guid hostKey,
            int attempts)
        {
            Debug.Log($"[NetworkLauncher] Starting as a client of {server.Endpoint()}");
            var transport = TransportOf(manager);
            transport.SetConnectionData(true, server.Address, server.Port);
            transport.MaxConnectAttempts = attempts;
            manager.NetworkConfig.ConnectionData = ConnectionRequestPayload.Of(new ConnectionRequest(_profile.Nickname, _profile.CharacterKey, _profile.Token, hostKey));
            _connection.ShowConnecting(server);
            _connection.Watch(manager, server, chosen);
            if (manager.StartClient())
                _networkHolder.NetworkManager.Value = manager;
            else
                _connection.ShowStartFailed(server);
        }

        private static UnityTransport TransportOf(NetworkManager manager) =>
            (UnityTransport)manager.NetworkConfig.NetworkTransport;
    }
}
