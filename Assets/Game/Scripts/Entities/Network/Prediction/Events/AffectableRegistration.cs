using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class AffectableRegistration<TState, TEffect> where TState : struct where TEffect : struct
    {
        public AffectableRegistration(
            Lifetime lifetime,
            INetworkLifetimedObject lifetimedObject,
            NetworkObject networkObject,
            IReadonlyAppliedState<TState> target,
            IAffectable<TEffect> affectable,
            IAffectables<TEffect> affectables)
        {
            lifetimedObject.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                affectables.AddLifetimed(spawnedLifetime, target, networkObject.NetworkObjectId, affectable));
        }
    }
}
