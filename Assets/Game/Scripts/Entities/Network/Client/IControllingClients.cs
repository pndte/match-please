using JetBrains.Lifetimes;

namespace Bw.Entities.Network
{
    public interface IControllingClients : IReadonlyViewableList<IClient>
    {
        public void Add(Lifetime lifetime, IClient client);
    }
}
