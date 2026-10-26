using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.LagCompensation
{
    public sealed class RewindableRegistrar
    {
        public RewindableRegistrar(
            Lifetime lifetime,
            INetworkLifetimedObject networkLifetimed,
            IRewindable rewindable,
            IRewindables rewindables)
        {
            networkLifetimed.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                rewindables.Track(spawnedLifetime, rewindable));
        }
    }
}
