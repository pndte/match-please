namespace Bw.Entities.Network.Prediction.Requests
{
    public readonly struct TickedInput<TInput> where TInput : struct
    {
        public readonly int Tick;
        public readonly TInput Input;
        public readonly TInput PreviousInput;

        public TickedInput(int tick, TInput input, TInput previousInput)
        {
            Tick = tick;
            Input = input;
            PreviousInput = previousInput;
        }
    }
}
