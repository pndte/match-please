namespace Bw.Entities.Network.Rollback
{
    public interface IStateDivergence<TState>
        where TState : struct
    {
        public bool Differs(TState predicted, TState actual);
    }
}
