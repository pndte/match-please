using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Events
{
    public interface IAffectables<TEffect> where TEffect : struct
    {
        public void AddLifetimed<TTarget>(Lifetime lifetime,
            TTarget target, ulong networkObjectId, IAffectable<TEffect> part) where TTarget : class;

        public bool TryGet<TTarget>(TTarget target, out ulong networkObjectId, out IAffectable<TEffect> affectable) where TTarget : class;
    }
}
