using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Events
{
    public interface IPredictionTargets<TEffect> where TEffect : struct
    {
        public void AddLifetimed<TState>(Lifetime lifetime, IReadonlyAppliedState<TState> target, ulong networkObjectId, IPredictionTarget<TEffect> predictionTarget)
            where TState : struct;
        public bool TryGet<TTarget>(TTarget target, out IPredictionTarget<TEffect> predictionTarget) where TTarget : class;
        public bool TryGet(ulong networkObjectId, out IPredictionTarget<TEffect> predictionTarget);
    }
}
