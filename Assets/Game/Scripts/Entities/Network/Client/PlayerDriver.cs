using System;

namespace Bw.Entities.Network
{
    public abstract class PlayerDriver
    {
        public static readonly PlayerDriver Server = new ServerDriver();

        private PlayerDriver()
        {
        }

        public static PlayerDriver Client(IClient client) =>
            new ClientDriver(client ?? throw new ArgumentNullException(nameof(client)));

        public abstract void Switch(Action<IClient> client, Action server);
        public abstract void Switch<TState>(TState state, Action<TState, IClient> client, Action<TState> server);

        private sealed class ClientDriver : PlayerDriver
        {
            private readonly IClient _client;

            public ClientDriver(IClient client)
            {
                _client = client;
            }

            public override void Switch(Action<IClient> client, Action server) =>
                client(_client);

            public override void Switch<TState>(TState state, Action<TState, IClient> client, Action<TState> server) =>
                client(state, _client);
        }

        private sealed class ServerDriver : PlayerDriver
        {
            public override void Switch(Action<IClient> client, Action server) =>
                server();

            public override void Switch<TState>(TState state, Action<TState, IClient> client, Action<TState> server) =>
                server(state);
        }
    }
}
