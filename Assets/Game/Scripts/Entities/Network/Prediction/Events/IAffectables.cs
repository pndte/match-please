using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Events
{
    public interface IAffectables<TEffect> where TEffect : struct
    {
        public void AddLifetimed<TState>(Lifetime lifetime, 
            IReadonlyAppliedState<TState> target, ulong networkObjectId, IAffectable<TEffect> affectable) where TState : struct;
        
        public bool TryGet<TTarget>(TTarget target, out ulong networkObjectId, out IAffectable<TEffect> affectable) where TTarget : class;
    }
}
