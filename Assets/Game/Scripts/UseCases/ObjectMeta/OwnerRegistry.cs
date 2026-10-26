using Bw.Entities;
using Bw.Entities.Players;
using JetBrains.Lifetimes;

namespace Bw.UseCases
{
    public sealed class OwnerRegistry : IOwnerRegistry
    {
        public IReadonlyViewableSet<IPlayer> Owners => _owners;

        private readonly BwViewableSet<IPlayer> _owners = new();

        public void AddOwner(Lifetime lifetime, IPlayer player) =>
            lifetime.Bracket(() => _owners.Add(player), () => _owners.Remove(player));
    }
}
