using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;

namespace Bw.UseCases.ObjectMeta.Network
{
    public sealed class ControlledByServerHandler
    {
        public ControlledByServerHandler(
            Lifetime lifetime,
            INetResultSender<bool> me,
            IControllerRegistry controllerRegistry,
            IClientPlayerCollection clientPlayers)
        {
            controllerRegistry.Users.View(lifetime, (userLifetime, user) =>
            {
                var client = clientPlayers.ByClient.Inverse[user]; //TODO: у бота клиента нет — когда появятся боты, игроков без клиента здесь пропускать
                me.SendTo(client, true);

                userLifetime.OnTermination(() =>
                    me.SendTo(client, false));
            });
        }
    }
}
