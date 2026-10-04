using Bw.Entities.Network.Variables;

namespace Bw.Entities.Network.Prediction.Requests
{
    public interface IPredictionStateResult<TState> where TState : struct
    {
        public INetSignal<TickedState<TState>> Received { get; }
    }
}
