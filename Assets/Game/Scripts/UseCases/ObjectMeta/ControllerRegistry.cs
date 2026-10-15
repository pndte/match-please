using Bw.Entities;
using Bw.Entities.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases
{
    public sealed class ControllerRegistry : IControllerRegistry
    {
        public IReadonlyViewableList<IPlayer> Users => _users;

        private readonly BwViewableList<IPlayer> _users = new();

        public void Set(Lifetime lifetime, IPlayer player)
        {
            _users.AddLifetimed(lifetime, player);
        }
    }
}
