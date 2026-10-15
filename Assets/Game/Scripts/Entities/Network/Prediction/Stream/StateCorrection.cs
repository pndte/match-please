namespace Bw.Entities.Network.Prediction.Stream
{
    public readonly struct StateCorrection<TState> where TState : struct
    {
        public readonly TState Before;
        public readonly TState After;

        public StateCorrection(TState before, TState after)
        {
            Before = before;
            After = after;
        }
    }
}
