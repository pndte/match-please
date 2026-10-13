namespace Bw.Entities.Simulation
{
    public interface IRestartable<in TState> where TState : struct
    {
        public void Restart(TState state);
    }
}
