namespace Bw.Entities.Network.Rollback
{
    public interface IStateDivergence<TState>
        where TState : struct
    {
        public bool Diverged(TState predicted, TState actual);
    }
}
