using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network
{
    public interface IClient
    {
        public ulong Id { get; } //TODO: id клиента должна знать IClientCollection (обратный поиск), а не сам клиент: роутеру и спавнеру брать его оттуда
        public string Nickname { get; }
        public IReadonlyProperty<ClientConnectionState> ConnectionState { get; }
        public void ChangeState(ClientConnectionState newState); //TODO: убрать, временно
    }

    public class Client : IClient
    {
        public ulong Id { get; }
        public string Nickname { get; }
        public IReadonlyProperty<ClientConnectionState> ConnectionState => InternalState;
        public void ChangeState(ClientConnectionState newState) //TODO: убрать, временно
        {
            InternalState.Value = newState;
        }

        protected readonly IViewableProperty<ClientConnectionState> InternalState = //TODO: rename maybe
            new ViewableProperty<ClientConnectionState>(ClientConnectionState.Loading);

        public Client(ulong id, string nickname)
        {
            Id = id;
            Nickname = nickname;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}