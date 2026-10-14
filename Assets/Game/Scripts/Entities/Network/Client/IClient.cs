using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network
{
    public interface IClient
    {
        public ulong Id { get; } //TODO: id клиента должна знать IClientCollection (обратный поиск), а не сам клиент: роутеру и спавнеру брать его оттуда
        public ConnectionRequest Request { get; }
        public IReadonlyProperty<ClientConnectionState> ConnectionState { get; }
        public void ChangeState(ClientConnectionState newState); //TODO: убрать, временно
    }

    public class Client : IClient
    {
        public ulong Id { get; }
        public ConnectionRequest Request { get; }
        public IReadonlyProperty<ClientConnectionState> ConnectionState => InternalState;
        public void ChangeState(ClientConnectionState newState) //TODO: убрать, временно
        {
            InternalState.Value = newState;
        }

        protected readonly IViewableProperty<ClientConnectionState> InternalState = //TODO: rename maybe
            new ViewableProperty<ClientConnectionState>(ClientConnectionState.Loading);

        public Client(ulong id, ConnectionRequest request)
        {
            Id = id;
            Request = request;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}