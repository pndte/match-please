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

                port = await RetryPort(port);
            }

            _room.ShowOpen(manager, port);
            _networkHolder.NetworkManager.Value = manager;
        }

        private UniTask Join(NetworkManager manager, LaunchChoice.Client server)
        {
            Connect(manager, server, server, JoinAttempts);
            return UniTask.CompletedTask;
        }

        private async UniTask OpenRoom(NetworkManager manager, LaunchChoice.Room room)
        {
            if (!BackgroundServer.Available)
            {
                _connection.ShowRoomServerMissing();
                return;
            }

            var port = room.Port;
            while (!LocalPorts.IsFree(port))
                port = await RetryPort(port);

            BackgroundServer.Open(_lifetime, port);
            Connect(manager, LaunchChoice.Client.Loopback(port), new LaunchChoice.Room(port), OwnRoomAttempts);
        }

        private async UniTask<ushort> RetryPort(ushort busyPort)
        {
            using var retrying = new LifetimeDefinition();
            return await _room.RetryPort(retrying.Lifetime, busyPort);
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
            int attempts)
        {
            Debug.Log($"[NetworkLauncher] Starting as a client of {server.Endpoint()}");
            var transport = TransportOf(manager);
            transport.SetConnectionData(true, server.Address, server.Port);
            transport.MaxConnectAttempts = attempts;
            manager.NetworkConfig.ConnectionData = ConnectionRequestPayload.Of(new ConnectionRequest(_profile.Nickname, _profile.CharacterKey, _profile.Token));
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
