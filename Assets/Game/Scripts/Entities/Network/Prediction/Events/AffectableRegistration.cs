using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class AffectableRegistration<TTarget, TAffectable, TEffect>
        where TTarget : class
        where TAffectable : class, IAffectable<TEffect>
        where TEffect : struct
    {
        public AffectableRegistration(
            Lifetime lifetime,
            INetworkLifetimedObject lifetimedObject,
            NetworkObject networkObject,
            TTarget target,
            TAffectable affectable,
            IAffectables<TEffect> affectables)
        {
            lifetimedObject.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                affectables.AddLifetimed(spawnedLifetime, target, networkObject.NetworkObjectId, affectable));
        }
    }
}
