using System;
using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using Unity.Netcode;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class OutcomePredictor<TTarget, TEffect> : IOutcomePredictor<TTarget, TEffect> where TTarget : class where TEffect : struct
    {
        public IReadonlyProperty<bool> Pending => _pending;

        private readonly ViewableProperty<bool> _pending = new(false);
        private readonly NetworkObject _initiator;
        private readonly IPredictionTargets<TTarget, TEffect> _targets;
        private readonly List<Entry> _entries = new();

        public OutcomePredictor(NetworkObject initiator, IPredictionTargets<TTarget, TEffect> targets)
        {
            _initiator = initiator;
            _targets = targets;
        }

        public void Predict(int tick, TTarget target, TEffect effect)
        {
            if (!_targets.TryGet(target, out var predictionTarget))
                throw new InvalidOperationException(
                    $"The {typeof(TTarget).Name} is not a spawned prediction target: install a target part on its object, such as EventPredictionTargetInstaller<{typeof(TTarget).Name}, its state type, {typeof(TEffect).Name}>, and predict on the object its parts are registered under.");

            var action = new ActionId(_initiator.NetworkObjectId, tick);
            if (!Booked(action, predictionTarget))
                _entries.Add(new Entry(action, predictionTarget));

            _pending.Value = true;
            predictionTarget.Show(new PredictedEffect<TEffect>(action, effect));
        }

        public void Answer(int tick, IReadOnlyList<AffectedTarget<IPredictionTarget<TEffect>, TEffect>> affected)
        {
            for (var index = _entries.Count - 1; index >= 0; index--)
            {
                var entry = _entries[index];
                if (entry.Action.Tick != tick)
                    continue;

                _entries.RemoveAt(index);
                ConfirmOrReject(entry, affected);
            }

            _pending.Value = _entries.Count > 0;
        }

        public void ExpireBefore(int tick)
        {
            for (var index = _entries.Count - 1; index >= 0; index--)
                if (_entries[index].Action.Tick < tick)
                    Reject(index);

            _pending.Value = _entries.Count > 0;
        }

        private bool Booked(ActionId action, IPredictionTarget<TEffect> target)
        {
            foreach (var entry in _entries)
                if (entry.Action.Equals(action) && entry.Target == target)
                    return true;

            return false;
        }

        private static void ConfirmOrReject(Entry entry, IReadOnlyList<AffectedTarget<IPredictionTarget<TEffect>, TEffect>> affected)
        {
            for (var index = 0; index < affected.Count; index++)
            {
                if (affected[index].Target != entry.Target)
                    continue;

                entry.Target.Confirm(entry.Action, affected[index].Effect);
                return;
            }

            entry.Target.Reject(entry.Action);
        }

        private void Reject(int index)
        {
            var entry = _entries[index];
            _entries.RemoveAt(index);
            entry.Target.Reject(entry.Action);
        }

        private readonly struct Entry
        {
            public readonly ActionId Action;
            public readonly IPredictionTarget<TEffect> Target;

            public Entry(ActionId action, IPredictionTarget<TEffect> target)
            {
                Action = action;
                Target = target;
            }
        }
    }
}
