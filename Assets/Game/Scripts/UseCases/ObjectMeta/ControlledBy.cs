using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases
{
    public class ControlledBy : IControlledBy
    {
        public IReadonlyProperty<bool> Me => _me;
        public IReadonlyViewableList<IPlayer> Users => _users;

        private readonly IViewableProperty<bool> _me;
        private readonly BwViewableList<IPlayer> _users;

        public ControlledBy()
        {
            _users = new BwViewableList<IPlayer>();
            _me = new ViewableProperty<bool>(false);
        }

        public void Set(Lifetime lifetime, IPlayer player)
        {
            _users.AddLifetimed(lifetime, player);
        }

        public class ClientNetworkHandler
        {
            public ClientNetworkHandler(
                Lifetime lifetime,
                INetSignal<bool> meSignal,
                ControlledBy controlledBy)
            {
                meSignal.Advise(lifetime, me =>
                {
                    controlledBy._me.Value = me;
                });
            }
        }

        public class ServerNetworkHandler
        {
            public ServerNetworkHandler(
                Lifetime lifetime,
                INetSignal<bool> meSignal,
                IControlledBy controlledBy,
                IClientPlayerCollection clientPlayers)
            {
                controlledBy.Users.View(lifetime, (userLifetime, userPlayer) =>
                {
                    var client = clientPlayers.ByClient.Inverse[userPlayer];
                    meSignal.FireTo(client, true);

                    userLifetime.OnTermination(() =>
                        meSignal.FireTo(client, false));
                });
            }
        }
    }
}
