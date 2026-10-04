namespace Bw.Entities.Simulation
{
    public interface IStateView<TState> where TState : struct
    {
        public void Show(TState from, TState to, float progress);
    }
}
