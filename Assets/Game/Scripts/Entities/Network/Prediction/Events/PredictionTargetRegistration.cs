using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class PredictionTargetRegistration<TTarget, TPredictionTarget, TEffect>
        where TTarget : class
        where TPredictionTarget : class, IPredictionTarget<TEffect>
        where TEffect : struct
    {
        public PredictionTargetRegistration(
            Lifetime lifetime,
            INetworkLifetimedObject lifetimedObject,
            NetworkObject networkObject,
            TTarget target,
            TPredictionTarget predictionTarget,
            IPredictionTargets<TEffect> targets)
        {
            lifetimedObject.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                targets.AddLifetimed(spawnedLifetime, target, networkObject.NetworkObjectId, predictionTarget));
        }
    }
}
