using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Match.Network
{
    public sealed class MatchPublisher
    {
        public MatchPublisher(
            Lifetime lifetime,
            INetworkLifetimedObject networkLifetimed,
            IReadonlyMatch match,
            IReadonlyScoreboard scoreboard,
            MatchTableWriter writer)
        {
            networkLifetimed.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
            {
                match.Period.Advise(spawnedLifetime, _ => writer.Write());
                scoreboard.ScoreChanged.Advise(spawnedLifetime, _ => writer.Write());
            });
        }
    }
}
