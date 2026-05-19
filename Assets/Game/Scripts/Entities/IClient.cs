namespace Bw.Entities
{
    public interface IClient
    {
        public ulong Id { get; }
    }

    public class Client : IClient
    {
        public ulong Id { get; }

        public Client(ulong id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}