namespace Bw.Entities.Network.Prediction.Stream
{
    public interface IInputExtrapolator<TInput> where TInput : struct
    {
        public TInput Continued(TInput last, int ticks);
    }
}
