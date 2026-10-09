namespace Bw.Entities.Network.Prediction.Events.Requests
{
    public readonly struct CausedState<TState> where TState : struct
    {
        public readonly TState State;
        public readonly StateCause Cause;

        public CausedState(TState state, StateCause cause)
        {
            State = state;
            Cause = cause;
        }
    }
}
