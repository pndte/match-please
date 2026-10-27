using System;
using System.Net;
using System.Net.Sockets;
using Bw.Entities.Network;

namespace Setup.Launch
{
    public abstract class LaunchChoice
    {
        public ushort Port { get; }

        private LaunchChoice(ushort port)
        {
            Port = port;
        }

        public abstract TResult Switch<TResult>(Func<Server, TResult> server, Func<Client, TResult> client, Func<Room, TResult> room);

        public abstract void Switch(Action<Server> server, Action<Client> client, Action<Room> room);

        public sealed class Server : LaunchChoice
        {
            public RoomSettings Settings { get; }

            public Server(ushort port, RoomSettings settings) : base(port)
            {
                Settings = settings;
            }

            public override TResult Switch<TResult>(Func<Server, TResult> server, Func<Client, TResult> client, Func<Room, TResult> room) =>
                server(this);

            public override void Switch(Action<Server> server, Action<Client> client, Action<Room> room) =>
                server(this);
        }

        public sealed class Client : LaunchChoice
        {
            private const string LoopbackAddress = "127.0.0.1";

            public string Address { get; }

            private Client(string address, ushort port) : base(port)
            {
                Address = address;
            }

            public static Client Loopback(ushort port) =>
                new(LoopbackAddress, port);

            public static bool TryParse(string text, ushort defaultPort, out Client client)
            {
                client = null;
                var parts = text.Trim().Split(':');
                var port = defaultPort;
                if (parts.Length > 2 || (parts.Length == 2 && !ushort.TryParse(parts[1], out port)))
                    return false;

                var address = parts[0];
                if (address.Split('.').Length != 4 || !IPAddress.TryParse(address, out var ip) || ip.AddressFamily != AddressFamily.InterNetwork)
                    return false;

                client = new Client(address, port);
                return true;
            }

            public override TResult Switch<TResult>(Func<Server, TResult> server, Func<Client, TResult> client, Func<Room, TResult> room) =>
                client(this);

            public override void Switch(Action<Server> server, Action<Client> client, Action<Room> room) =>
                client(this);
        }

        public sealed class Room : LaunchChoice
        {
            public RoomSettings Settings { get; }

            public Room(ushort port, RoomSettings settings) : base(port)
            {
                Settings = settings;
            }

            public override TResult Switch<TResult>(Func<Server, TResult> server, Func<Client, TResult> client, Func<Room, TResult> room) =>
                room(this);

            public override void Switch(Action<Server> server, Action<Client> client, Action<Room> room) =>
                room(this);
        }
    }
}
