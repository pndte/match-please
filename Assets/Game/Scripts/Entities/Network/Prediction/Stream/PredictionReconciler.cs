using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class PredictionReconciler<TInput, TState>
        where TInput : struct
        where TState : struct
    {
        private const int MaxPendingInputs = 128;

        private readonly Queue<TickedInput<TInput>> _pending = new();
        private readonly ISimulation<TState> _simulation;
        private readonly ISimulator<TInput, TState> _simulator;

        private int _lastReconciledTick;

        public PredictionReconciler(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IPredictionInputRequest<TInput> inputRequest,
            IPredictionStateResult<TState> stateResult,
            ISimulation<TState> simulation,
            ISimulator<TInput, TState> simulator)
        {
            _simulation = simulation;
            _simulator = simulator;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime => //TODO: у чужого объекта локальное состояние не обновлялось (снимки идут только в показ) — получив управление (подбор оружия), клиент до первого состояния сервера предсказывает от устаревшего: патроны, счётчик выстрелов
            {
                inputRequest.Requested.Advise(controlledLifetime, Remember);
                stateResult.Received.Advise(controlledLifetime, Reconcile);
                controlledLifetime.OnTermination(_pending.Clear);
            });
        }

        private void Remember(TickedInput<TInput> request)
        {
            _pending.Enqueue(request);
            if (_pending.Count > MaxPendingInputs)
                _pending.Dequeue();
        }

        private void Reconcile(TickedState<TState> result)
        {
            if (result.Tick <= _lastReconciledTick)
                return;

            _lastReconciledTick = result.Tick;
            while (_pending.Count > 0 && _pending.Peek().Tick <= result.Tick)
                _pending.Dequeue();

            var state = result.State;
            foreach (var request in _pending)
                state = _simulator.Step(state, request.Input);

            _simulation.Apply(state);
        }
    }
}
