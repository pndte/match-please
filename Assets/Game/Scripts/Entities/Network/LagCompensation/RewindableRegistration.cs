using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.LagCompensation
{
    public sealed class RewindableRegistration
    {
        public RewindableRegistration(
            Lifetime lifetime,
            INetworkLifetimedObject networkObject,
            IRewindable rewindable,
            IRewindableCollection rewindables)
        {
            networkObject.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                rewindables.AddLifetimed(spawnedLifetime, rewindable));
        }
    }
}
