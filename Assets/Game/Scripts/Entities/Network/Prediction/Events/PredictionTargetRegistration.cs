using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class PredictionTargetRegistration<TState, TEffect> where TState : struct where TEffect : struct
    {
        public PredictionTargetRegistration(
            Lifetime lifetime,
            INetworkLifetimedObject lifetimedObject,
            NetworkObject networkObject,
            IReadonlyAppliedState<TState> target,
            IPredictionTarget<TEffect> predictionTarget,
            IPredictionTargets<TEffect> targets)
        {
            lifetimedObject.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                targets.AddLifetimed(spawnedLifetime, target, networkObject.NetworkObjectId, predictionTarget));
        }
    }
}
