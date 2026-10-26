using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class AffectableRegistrar<TTarget, TEffect>
        where TTarget : class
        where TEffect : struct
    {
        public AffectableRegistrar(
            Lifetime lifetime,
            INetworkLifetimedObject lifetimedObject,
            NetworkObject networkObject,
            TTarget target,
            IAffectable<TEffect> affectable,
            IAffectables<TTarget, TEffect> affectables)
        {
            lifetimedObject.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                affectables.AddLifetimed(spawnedLifetime, target, networkObject.NetworkObjectId, affectable));
        }
    }
}
