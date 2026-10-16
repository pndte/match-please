using System;
using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class PredictionReconciler<TInput, TState> : IStateCorrections<TState>
        where TInput : struct
        where TState : struct, IEquatable<TState>
    {
        private const int MaxPendingInputs = 128;

        public ISource<StateCorrection<TState>> Corrected => _corrected;

        private readonly Signal<StateCorrection<TState>> _corrected = new();
        private readonly Queue<TickedInput<TInput>> _pending = new();
        private readonly ISimulation<TState> _simulation;
        private readonly ISimulator<TInput, TState> _simulator;
        private readonly IScheduledCollection _scheduled;
        private readonly IPredictedBodyCollection _predictedBodies;
        private readonly INetworkTicks _ticks;

        private int _lastReconciledTick;

        public PredictionReconciler(
            Lifetime lifetime,
            IControlledBy controlledBy,
            INetRequestSender<TickedInput<TInput>> input,
            INetResultReceiver<TickedState<TState>> state,
            ISimulation<TState> simulation,
            ISimulator<TInput, TState> simulator,
            IScheduledCollection scheduled,
            IPredictedBodyCollection predictedBodies,
            INetworkTicks ticks)
        {
            _simulation = simulation;
            _simulator = simulator;
            _scheduled = scheduled;
            _predictedBodies = predictedBodies;
            _ticks = ticks;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime => //TODO: у чужого объекта локальное состояние не обновлялось (снимки идут только в показ) — получив управление (подбор оружия), клиент до первого состояния сервера предсказывает от устаревшего: патроны, счётчик выстрелов
            {
                input.Sent.Advise(controlledLifetime, Remember);
                state.Received.Advise(controlledLifetime, Reconcile);
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
            {
                _scheduled.PoseAt(request.Tick);
                _predictedBodies.PoseFor(request.Tick);
                state = _simulator.Step(state, request.Input);
            }

            _scheduled.PoseAt(_ticks.Current);
            _predictedBodies.PoseFor(_ticks.Current);
            var before = _simulation.State.Value;
            _simulation.Apply(state); //TODO: показ сглаживает поправку только у тел (SmoothedStateView) — прицел оружия и предмета в руках при поправке перескакивает
            if (!before.Equals(state))
                _corrected.Fire(new StateCorrection<TState>(before, state));
        }
    }
}
