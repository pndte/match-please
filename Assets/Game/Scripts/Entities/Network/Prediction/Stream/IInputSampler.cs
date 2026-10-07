namespace Bw.Entities.Network.Prediction.Stream
{
    public interface IInputSampler<TInput> where TInput : struct
    {
        public TInput Sample();
    }
}
