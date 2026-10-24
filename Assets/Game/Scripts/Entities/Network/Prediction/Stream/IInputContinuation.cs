namespace Bw.Entities.Network.Prediction.Stream
{
    public interface IInputContinuation<TInput> where TInput : struct
    {
        public TInput Continued(TInput last, int ticks);
    }
}
