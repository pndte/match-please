namespace Bw.Entities.Network.Prediction.Stream
{
    public interface IRemoteInputs<TInput>
        where TInput : struct
    {
        public bool TryTakeInputAt(int tick, out TInput input);
    }
}
