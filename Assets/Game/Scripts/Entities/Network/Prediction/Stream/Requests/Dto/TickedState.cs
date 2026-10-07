namespace Bw.Entities.Network.Prediction.Stream.Requests
{
    public readonly struct TickedState<TState> where TState : struct
    {
        public readonly int Tick;
        public readonly TState State;

        public TickedState(int tick, TState state)
        {
            Tick = tick;
            State = state;
        }
    }
}
