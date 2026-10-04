namespace Bw.Entities.Simulation
{
    public interface ISimulator<TInput, TState>
        where TInput : struct
        where TState : struct
    {
        public TState Step(TState state, TInput input);
    }
}
