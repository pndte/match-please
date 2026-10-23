using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Mortality.Guarding;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Mortality.Network.Guarding
{
    public sealed class GuardPublisher
    {
        public GuardPublisher(
            Lifetime lifetime,
            INetworkLifetimedObject networkLifetimed,
            IReadonlyGuard guard,
            INetEntries entries,
            NetPropertyDeclaration<GuardState> declaration)
        {
            var published = entries.Get(declaration);
            networkLifetimed.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                guard.State.Advise(spawnedLifetime, state => published.Value = state));
        }
    }
}
