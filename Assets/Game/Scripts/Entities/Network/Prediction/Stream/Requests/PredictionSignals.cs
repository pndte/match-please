using Bw.Entities.Network.Variables;

namespace Bw.Entities.Network.Prediction.Stream.Requests
{
    public sealed class PredictionSignals<TInput, TState>
        : IPredictionInputRequest<TInput>, IPredictionStateResult<TState>, IInputMarginResult
        where TInput : struct
        where TState : struct
    {
        public INetSignal<TickedInput<TInput>> Requested { get; }
        public INetSignal<TickedState<TState>> Received { get; }
        public INetSignal<int> InputMarginReceived { get; }
        INetSignal<int> IInputMarginResult.Received => InputMarginReceived;

        public PredictionSignals(
            INetSignal<TickedInput<TInput>> requested,
            INetSignal<TickedState<TState>> received,
            INetSignal<int> inputMarginReceived)
        {
            Requested = requested;
            Received = received;
            InputMarginReceived = inputMarginReceived;
        }
    }
}
