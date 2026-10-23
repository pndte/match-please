using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.ObjectMeta.Network
{
    public sealed class ControllerDrivers
    {
        public ControllerDrivers(
            Lifetime lifetime,
            IControllerRegistry controllerRegistry,
            IReadonlyPlayerDrivers drivers,
            IControlledBy controlledBy,
            IControllingClients controllingClients)
        {
            controllerRegistry.Users.View(lifetime, (userLifetime, user) =>
                drivers.Of(user).View(userLifetime, (drivenLifetime, driver) =>
                    driver.Switch(
                        client => controllingClients.Add(drivenLifetime, client),
                        () => controlledBy.SetMe(drivenLifetime))));
        }
    }
}
