using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;

namespace Bw.UseCases.ObjectMeta.Network
{
    public sealed class ControlledByServerHandler
    {
        public ControlledByServerHandler(Lifetime lifetime, INetResultSender<bool> me, IControllingClients controllingClients)
        {
            controllingClients.View(lifetime, (clientLifetime, client) =>
            {
                me.SendTo(client, true);
                clientLifetime.OnTermination(() => me.SendTo(client, false));
            });
        }
    }
}
