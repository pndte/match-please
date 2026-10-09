using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class PredictedState<TState, TEffect> : IPredictionTarget<TEffect> where TState : struct where TEffect : struct
    {
        private readonly IAppliedState<TState> _state;
        private readonly IPredictionRules<TState, TEffect> _rules;
        private readonly List<PredictedEffect<TEffect>> _confirmed = new();
        private readonly List<PredictedEffect<TEffect>> _pending = new();

        private TState _authoritative;

        public PredictedState(
            Lifetime lifetime,
            INetResultReceiver<CausedState<TState>> authoritative,
            IAppliedState<TState> state,
            IPredictionRules<TState, TEffect> rules)
        {
            _state = state;
            _rules = rules;
            _authoritative = state.Current;

            authoritative.Received.Advise(lifetime, Receive); //TODO: состояние применяется сразу по приходу, а выстрел, который его вызвал, зрители видят интерполяцией позже — у них вспышка удара опережает трассер стрелка; показывать по тику интерполяции причины
        }

        public void Show(PredictedEffect<TEffect> effect)
        {
            var shown = _state.Current;
            _pending.Add(effect);
            _state.Apply(_rules.Present(shown, Predicted(), ChangeCause<TState, TEffect>.Predicted(effect.Value)));
        }

        public void Confirm(ActionId action, TEffect actual)
        {
            if (!Waiting(_pending, action))
                return;

            Withdraw(_pending, action);
            _confirmed.Add(new PredictedEffect<TEffect>(action, actual));
            Correct();
        }

        public void Reject(ActionId action)
        {
            if (!Waiting(_pending, action))
                return;

            Withdraw(_pending, action);
            Correct();
        }

        private void Receive(CausedState<TState> message)
        {
            _authoritative = message.State;
            message.Cause.Switch(this, none: static _ => { }, action: static (self, action) => self.Settle(action));
            _state.Apply(_rules.Present(_state.Current, Predicted(), ChangeCause<TState, TEffect>.Authoritative(message.State)));
        }

        private void Settle(ActionId action)
        {
            Withdraw(_confirmed, action);
            Withdraw(_pending, action);
        }

        private void Correct()
        {
            var shown = _state.Current;
            _state.Apply(_rules.Present(shown, Predicted(), ChangeCause<TState, TEffect>.Correction));
        }

        private TState Predicted()
        {
            var state = _authoritative;
            foreach (var effect in _confirmed)
                state = _rules.Predict(state, effect.Value);

            foreach (var effect in _pending)
                state = _rules.Predict(state, effect.Value);

            return state;
        }

        private static bool Waiting(List<PredictedEffect<TEffect>> effects, ActionId action)
        {
            foreach (var effect in effects)
                if (effect.Action.Equals(action))
                    return true;

            return false;
        }

        private static void Withdraw(List<PredictedEffect<TEffect>> effects, ActionId action)
        {
            for (var index = effects.Count - 1; index >= 0; index--)
                if (effects[index].Action.Equals(action))
                    effects.RemoveAt(index);
        }
    }
}
