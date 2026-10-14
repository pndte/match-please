using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Events
{
    public interface IAffectables<TTarget, TEffect> where TTarget : class where TEffect : struct
    {
        public void AddLifetimed(
            Lifetime lifetime,
            TTarget target,
            ulong networkObjectId,
            IAffectable<TEffect> part);

        public bool TryGet(TTarget target, out ulong networkObjectId, out IAffectable<TEffect> affectable);
    }
}
