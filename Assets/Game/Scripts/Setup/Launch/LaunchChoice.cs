using System;
using System.Net;
using System.Net.Sockets;

namespace Setup.Launch
{
    public abstract class LaunchChoice
    {
        public ushort Port { get; }

        private LaunchChoice(ushort port)
        {
            Port = port;
        }

        public abstract TResult Switch<TResult>(Func<Server, TResult> server, Func<Client, TResult> client);

        public abstract void Switch(Action<Server> server, Action<Client> client);

        public sealed class Server : LaunchChoice
        {
            public Server(ushort port) : base(port)
            {
            }

            public override TResult Switch<TResult>(Func<Server, TResult> server, Func<Client, TResult> client) =>
                server(this);

            public override void Switch(Action<Server> server, Action<Client> client) =>
                server(this);
        }

        public sealed class Client : LaunchChoice
        {
            public string Address { get; }

            private Client(string address, ushort port) : base(port)
            {
                Address = address;
            }

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

            public override TResult Switch<TResult>(Func<Server, TResult> server, Func<Client, TResult> client) =>
                client(this);

            public override void Switch(Action<Server> server, Action<Client> client) =>
                client(this);
        }
    }
}
