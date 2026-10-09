using JetBrains.Collections.Viewable;

namespace Bw.Entities
{
    public interface IReadonlyAppliedState<TState> where TState : struct
    {
        public TState Current { get; }
        public ISource<TState> Applied { get; }
    }

    public interface IAppliedState<TState> : IReadonlyAppliedState<TState> where TState : struct
    {
        public void Apply(TState state);
    }
}
