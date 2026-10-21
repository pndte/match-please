using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.ObjectMeta.Network
{
    public sealed class OwnershipServerHandler
    {
        public OwnershipServerHandler(
            Lifetime lifetime,
            INetResultSender<bool> mine,
            IOwnerRegistry ownerRegistry,
            IReadonlyPlayerDrivers drivers,
            IOwnership ownership)
        {
            ownerRegistry.Owners.View(lifetime, (ownerLifetime, owner) =>
                drivers.Of(owner).View(ownerLifetime, (drivenLifetime, driver) =>
                    driver.Switch(
                        client =>
                        {
                            mine.SendTo(client, true);

                            drivenLifetime.OnTermination(() =>
                                mine.SendTo(client, false));
                        },
                        () => ownership.SetMine(drivenLifetime))));
        }
    }
}
