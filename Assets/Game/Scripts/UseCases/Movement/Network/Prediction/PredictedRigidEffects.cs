using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PredictedRigidEffects<TEffect> : IPredictionTarget<TEffect>, IRigidStateEffects where TEffect : struct
    {
        private const int KeptTicks = 64;

        public ISource<int> Changed => _changed;

        private readonly Signal<int> _changed = new();
        private readonly List<Effect> _effects = new();
        private readonly IEffectRules<RigidState, TEffect> _rules;

        public PredictedRigidEffects(Lifetime lifetime, IEffectRules<RigidState, TEffect> rules, INetworkTicks ticks)
        {
            _rules = rules;

            ticks.Ticked(TickPhase.Default).Advise(lifetime, DropOld);
        }

        public void Show(PredictedEffect<TEffect> effect)
        {
            _effects.Add(new Effect(effect.Action, effect.Value));
            _changed.Fire(effect.Action.Tick);
        }

        public void Confirm(ActionId action, TEffect actual)
        {
            var index = IndexOf(action);
            if (index < 0)
                return;

            _effects[index] = new Effect(action, actual);
            _changed.Fire(action.Tick);
        }

        public void Reject(ActionId action)
        {
            var index = IndexOf(action);
            if (index < 0)
                return;

            _effects.RemoveAt(index);
            _changed.Fire(action.Tick);
        }

        public bool TryGetAffected(int tick, RigidState state, out RigidState affected)
        {
            affected = state;
            var found = false;
            for (var index = 0; index < _effects.Count; index++)
            {
                if (_effects[index].Action.Tick != tick)
                    continue;

                affected = _rules.Apply(affected, _effects[index].Value);
                found = true;
            }

            return found;
        }

        private void DropOld(int tick)
        {
            for (var index = _effects.Count - 1; index >= 0; index--)
                if (_effects[index].Action.Tick < tick - KeptTicks)
                    _effects.RemoveAt(index);
        }

        private int IndexOf(ActionId action)
        {
            for (var index = 0; index < _effects.Count; index++)
                if (_effects[index].Action.Equals(action))
                    return index;

            return -1;
        }

        private readonly struct Effect
        {
            public readonly ActionId Action;
            public readonly TEffect Value;

            public Effect(ActionId action, TEffect value)
            {
                Action = action;
                Value = value;
            }
        }
    }
}
