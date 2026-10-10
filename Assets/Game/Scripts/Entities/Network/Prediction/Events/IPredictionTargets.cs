using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Events
{
    public interface IPredictionTargets<TEffect> where TEffect : struct
    {
        public void AddLifetimed<TTarget>(Lifetime lifetime, 
            TTarget target, ulong networkObjectId, IPredictionTarget<TEffect> part) where TTarget : class;
        public bool TryGet<TTarget>(TTarget target, out IPredictionTarget<TEffect> predictionTarget) where TTarget : class;
        public bool TryGet(ulong networkObjectId, out IPredictionTarget<TEffect> predictionTarget);
    }
}
