using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class PredictionTargetRegistrar<TTarget, TEffect>
        where TTarget : class
        where TEffect : struct
    {
        public PredictionTargetRegistrar(
            Lifetime lifetime,
            INetworkLifetimedObject lifetimedObject,
            NetworkObject networkObject,
            TTarget target,
            IPredictionTarget<TEffect> predictionTarget,
            IPredictionTargets<TTarget, TEffect> targets)
        {
            lifetimedObject.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                targets.AddLifetimed(spawnedLifetime, target, networkObject.NetworkObjectId, predictionTarget));
        }
    }
}
