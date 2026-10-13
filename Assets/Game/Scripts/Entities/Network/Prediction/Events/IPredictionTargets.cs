using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Events
{
    public interface IPredictionTargets<TTarget, TEffect> where TTarget : class where TEffect : struct
    {
        public void AddLifetimed(Lifetime lifetime,
            TTarget target, ulong networkObjectId, IPredictionTarget<TEffect> part);
        public bool TryGet(TTarget target, out IPredictionTarget<TEffect> predictionTarget);
        public bool TryGet(ulong networkObjectId, out IPredictionTarget<TEffect> predictionTarget);
    }
}
