using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Rollback
{
    public sealed class RollbackPartRegistration<TState> where TState : struct
    {
        public RollbackPartRegistration(
            Lifetime lifetime,
            IPredictedBy predictedBy,
            RollbackPart<TState> part,
            IRollbackParts parts)
        {
            predictedBy.Me.WhenTrue(lifetime, predictedLifetime => parts.Track(predictedLifetime, part));
        }
    }
}
