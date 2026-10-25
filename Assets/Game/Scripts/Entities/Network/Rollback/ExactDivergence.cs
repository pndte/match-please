using System;

namespace Bw.Entities.Network.Rollback
{
    public sealed class ExactDivergence<TState> : IStateDivergence<TState>
        where TState : struct, IEquatable<TState>
    {
        public bool Diverged(TState predicted, TState actual) =>
            !predicted.Equals(actual);
    }
}
