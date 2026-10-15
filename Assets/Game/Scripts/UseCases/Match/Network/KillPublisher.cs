using Bw.Entities.Network.Variables;
using Bw.UseCases.Match.Network.Requests;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Match.Network
{
    public sealed class KillPublisher
    {
        private readonly KillNotices _notices;
        private readonly INetResultSender<KillNotice> _sender;

        public KillPublisher(
            Lifetime lifetime,
            IKills kills,
            KillNotices notices,
            INetResultSender<KillNotice> sender)
        {
            _notices = notices;
            _sender = sender;

            kills.Scored.Advise(lifetime, Publish);
        }

        private void Publish(Kill kill) =>
            _sender.Broadcast(_notices.Of(kill));
    }
}
