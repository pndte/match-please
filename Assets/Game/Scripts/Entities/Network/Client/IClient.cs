namespace Bw.Entities.Network
{
    public interface IClient
    {
        public ConnectionRequest Request { get; }
    }

    public sealed class Client : IClient
    {
        public ConnectionRequest Request { get; }

        public Client(ConnectionRequest request)
        {
            Request = request;
        }

        public override string ToString()
        {
            return Request.Nickname;
        }
    }
}
