using JetBrains.Collections.Viewable;

namespace Bw.Entities.Simulation
{
    public interface IRestorable<TState> where TState : struct
    {
        public IReadonlyProperty<TState> State { get; }

        public void Apply(TState state);
    }
}
