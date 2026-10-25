using Bw.Entities.Network.Rollback;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class PredictedAffectableRegistration<TTarget, TEffect>
        where TTarget : class
        where TEffect : struct
    {
        public PredictedAffectableRegistration(
            Lifetime lifetime,
            IPredictedBy predictedBy,
            NetworkObject networkObject,
            TTarget target,
            IAffectable<TEffect> affectable,
            IAffectables<TTarget, TEffect> affectables)
        {
            predictedBy.Me.WhenTrue(lifetime, predictedLifetime =>
                affectables.AddLifetimed(predictedLifetime, target, networkObject.NetworkObjectId, affectable));
        }
    }
}
