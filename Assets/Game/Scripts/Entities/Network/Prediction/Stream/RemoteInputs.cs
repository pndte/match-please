using System;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class RemoteInputs<TInput> : IRemoteInputs<TInput>, IDivergenceSource where TInput : struct
    {
        private readonly TickHistory<TInput> _relayed;
        private readonly TickHistory<TInput> _taken;
        private readonly IInputExtrapolator<TInput> _extrapolator;
        private readonly IInputDivergence<TInput> _divergence;

        public RemoteInputs(
            Lifetime lifetime,
            INetResultReceiver<TickedInput<TInput>> relayed,
            IInputExtrapolator<TInput> extrapolator,
            IInputDivergence<TInput> divergence,
            WorldRollbackConfig config)
        {
            _extrapolator = extrapolator;
            _divergence = divergence;
            _relayed = new TickHistory<TInput>(config.HistoryTicks);
            _taken = new TickHistory<TInput>(config.HistoryTicks);

            relayed.Received.Advise(lifetime, Remember);
        }

        public bool TryTakeInputAt(int tick, out TInput input)
        {
            if (!_relayed.TryGetLatest(tick, out var relayedTick, out var relayedInput))
            {
                input = default;
                return false;
            }

            input = relayedTick == tick ? relayedInput : _extrapolator.Continued(relayedInput, tick - relayedTick);
            _taken.Record(tick, input);
            return true;
        }

        public bool TryGetDivergence(out int tick)
        {
            var newest = Math.Min(_taken.NewestTick, _relayed.NewestTick);
            if (newest != int.MinValue)
                for (var taken = newest - _taken.Capacity + 1; taken <= newest; taken++)
                    if (Diverged(taken))
                    {
                        tick = taken - 1;
                        return true;
                    }

            tick = default;
            return false;
        }

        private bool Diverged(int tick) =>
            _taken.TryGetAt(tick, out var guessed) && _relayed.TryGetAt(tick, out var relayed) && _divergence.Diverged(guessed, relayed);

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
