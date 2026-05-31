using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Players;
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
                IDtoSource<TargetedBool> dtoSource,
                Ownership ownership)
            {
                dtoSource.Value.Advise(lifetime, dto => ownership._mine.Value = dto.Value);
            }
        }

        public class ServerNetworkHandler
        {
            public ServerNetworkHandler(
                Lifetime lifetime,
                IDtoBroadcaster<TargetedBool> dtoBroadcaster,
                IOwnershipController ownershipController,
                IClientPlayerCollection clientPlayers)
            {
                ownershipController.Owners.View(lifetime, (ownerLifetime, ownerPlayer) =>
                {
                    var client = clientPlayers.ByClient.Inverse[ownerPlayer];
                    dtoBroadcaster.Fire(new TargetedBool(client, true));

                    ownerLifetime.OnTermination(() =>
                        dtoBroadcaster.Fire(new TargetedBool(client, false)));
                });
            }
        }
    }
}
