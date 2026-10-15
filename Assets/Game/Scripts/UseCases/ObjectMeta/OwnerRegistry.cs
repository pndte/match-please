using Bw.Entities;
using Bw.Entities.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases
{
    public sealed class OwnerRegistry : IOwnerRegistry
    {
        public IReadonlyViewableList<IPlayer> Owners => _owners;

        private readonly BwViewableList<IPlayer> _owners = new();

        public void AddOwner(Lifetime lifetime, IPlayer player)
        {
            _owners.AddLifetimed(lifetime, player);
        }
    }
}
