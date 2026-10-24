namespace Bw.Entities.Network.Prediction.Stream
{
    public interface IRemoteInputs<TInput>
        where TInput : struct
    {
        public bool TryGetInputAt(int tick, out TInput input);
    }
}
