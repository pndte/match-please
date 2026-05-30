using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
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
                IDtoSource<ControlledByDto> dtoSource,
                INetworkHolder networkHolder,
                ControlledBy controlledBy)
            {
                var localClientId = networkHolder.NetworkManager.Value.LocalClientId;
                dtoSource.Value.Advise(lifetime, dto =>
                {
                    if (dto.RecipientClientId != localClientId) return;

                    controlledBy._me.Value = dto.Mine;
                });
            }
        }

        public class ServerNetworkHandler
        {
            public ServerNetworkHandler(
                Lifetime lifetime,
                IDtoBroadcaster<ControlledByDto> dtoBroadcaster,
                IControlledBy controlledBy,
                IClientPlayerCollection clientPlayers)
            {
                controlledBy.Users.View(lifetime, (userLifetime, userPlayer) =>
                {
                    var client = clientPlayers.ByClient.Inverse[userPlayer];
                    dtoBroadcaster.Fire(new ControlledByDto(client.Id, true));

                    userLifetime.OnTermination(() =>
                        dtoBroadcaster.Fire(new ControlledByDto(client.Id, false)));
                });
            }
        }
    }
}
