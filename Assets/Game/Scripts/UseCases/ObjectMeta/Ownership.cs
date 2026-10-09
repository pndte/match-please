using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Entities.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases
{
    public class Ownership : IOwnershipController, IOwnership
    {
        public IReadonlyProperty<bool> Mine => _mine;
        public IReadonlyViewableList<IPlayer> Owners => _owners;

        private readonly IViewableProperty<bool> _mine;
        private readonly BwViewableList<IPlayer> _owners;

        public Ownership()
        {
            _owners = new BwViewableList<IPlayer>();
            _mine = new ViewableProperty<bool>(false);
        }

        public void AddOwner(Lifetime lifetime, IPlayer player)
        {
            _owners.AddLifetimed(lifetime, player);
        }

        public class ClientNetworkHandler //TODO: в идеале это всё декомпозировать надо и избавиться от Ownership прямо в конструкторе, костыль по сути
        {
            public ClientNetworkHandler(
                Lifetime lifetime,
                INetResultReceiver<bool> mine,
                Ownership ownership)
            {
                mine.Received.Advise(lifetime, value => ownership._mine.Value = value);
            }
        }

        public class ServerNetworkHandler
        {
            public ServerNetworkHandler(
                Lifetime lifetime,
                INetResultSender<bool> mine,
                IOwnershipController ownershipController,
                IClientPlayerCollection clientPlayers)
            {
                ownershipController.Owners.View(lifetime, (ownerLifetime, ownerPlayer) =>
                {
                    var client = clientPlayers.ByClient.Inverse[ownerPlayer]; //TODO: у бота клиента нет — когда появятся боты, игроков без клиента здесь пропускать
                    mine.SendTo(client, true);

                    ownerLifetime.OnTermination(() =>
                        mine.SendTo(client, false));
                });
            }
        }
    }
}
