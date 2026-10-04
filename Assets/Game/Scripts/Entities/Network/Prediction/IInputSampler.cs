namespace Bw.Entities.Network.Prediction
{
    public interface IInputSampler<TInput> where TInput : struct
    {
        public TInput Sample();
    }
}
