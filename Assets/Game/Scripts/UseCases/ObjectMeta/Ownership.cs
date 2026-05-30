using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.UseCases.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases
{
    public class Ownership : IOwnership
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
                IDtoSource<OwnershipDto> dtoSource, 
                INetworkHolder networkHolder,
                Ownership ownership)
            {
                var localClientId = networkHolder.NetworkManager.Value.LocalClientId;
                dtoSource.Value.Advise(lifetime, dto =>
                {
                    if (dto.RecipientClientId != localClientId) return;

                    ownership._mine.Value = dto.IsOwner;
                });
            }
        }

        public class ServerNetworkHandler
        {
            public ServerNetworkHandler(
                Lifetime lifetime, 
                IDtoBroadcaster<OwnershipDto> dtoBroadcaster,
                IOwnership ownership,
                IClientPlayerCollection clientPlayers)
            {
                ownership.Owners.View(lifetime, (ownerLifetime, ownerPlayer) =>
                {
                    var client = clientPlayers.ByClient.Inverse[ownerPlayer];
                    dtoBroadcaster.Fire(new OwnershipDto(client.Id, true));

                    ownerLifetime.OnTermination(() =>
                        dtoBroadcaster.Fire(new OwnershipDto(client.Id, false)));
                });
            }
        }
    }
}
