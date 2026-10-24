namespace Bw.Entities.Network.Prediction.Stream
{
    public interface IInputPolicy<TInput> where TInput : struct
    {
        public bool IsValid(TInput input);

        public TInput CaughtUp(
            TInput input,
            TInput lastInput,
            TInput heard,
            int heardTicksAgo);
    }
}
