using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.ObjectMeta.Network
{
    public sealed class ControlledByServerHandler
    {
        public ControlledByServerHandler(
            Lifetime lifetime,
            INetResultSender<bool> me,
            IControllerRegistry controllerRegistry,
            IReadonlyPlayerDrivers drivers,
            IControlledBy controlledBy,
            IControllingClients controllingClients)
        {
            controllerRegistry.Users.View(lifetime, (userLifetime, user) =>
                drivers.Of(user).View(userLifetime, (drivenLifetime, driver) =>
                    driver.Switch(
                        client =>
                        {
                            controllingClients.Add(drivenLifetime, client);
                            me.SendTo(client, true);

                            drivenLifetime.OnTermination(() =>
                                me.SendTo(client, false));
                        },
                        () => controlledBy.SetMe(drivenLifetime))));
        }
    }
}
