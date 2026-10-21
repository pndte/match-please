using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network
{
    public sealed class ControllingClients : BwViewableList<IClient>, IControllingClients
    {
        public void Add(Lifetime lifetime, IClient client) =>
            this.AddLifetimed(lifetime, client);
    }
}
