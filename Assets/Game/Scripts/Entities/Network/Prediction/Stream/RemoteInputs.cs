using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class RemoteInputs<TInput> : IRemoteInputs<TInput> where TInput : struct
    {
        private readonly TickHistory<TInput> _relayed;
        private readonly IInputContinuation<TInput> _continuation;

        public RemoteInputs(
            Lifetime lifetime,
            INetResultReceiver<TickedInput<TInput>> relayed,
            IInputContinuation<TInput> continuation,
            WorldRollbackConfig config)
        {
            _continuation = continuation;
            _relayed = new TickHistory<TInput>(config.HistoryTicks);

            relayed.Received.Advise(lifetime, Remember);
        }

        public bool TryGetInputAt(int tick, out TInput input)
        {
            if (!_relayed.TryGetLatest(tick, out var relayedTick, out var relayedInput))
            {
                input = default;
                return false;
            }

            input = relayedTick == tick ? relayedInput : _continuation.Continued(relayedInput, tick - relayedTick);
            return true;
        }

        private void Remember(TickedInput<TInput> relayed)
        {
            var newest = _relayed.NewestTick;
            if (newest != int.MinValue && relayed.Tick <= newest)
                return;

            if (newest != int.MinValue && relayed.Tick - 1 > newest)
                _relayed.Record(relayed.Tick - 1, relayed.PreviousInput);

            _relayed.Record(relayed.Tick, relayed.Input);
        }
    }
}
