using Bw.Entities.Network.Variables;

namespace Bw.Entities.Network.Prediction.Stream.Requests
{
    public interface IPredictionInputRequest<TInput> where TInput : struct
    {
        public INetSignal<TickedInput<TInput>> Requested { get; }
    }
}
