using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Variables;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class AuthoritativeState<TState, TEffect> : IAffectable<TEffect> where TState : struct where TEffect : struct
    {
        private readonly IAppliedState<TState> _state;
        private readonly IEffectRules<TState, TEffect> _rules;
        private readonly INetResultSender<CausedState<TState>> _result;

        public AuthoritativeState(
            IAppliedState<TState> state,
            IEffectRules<TState, TEffect> rules,
            INetResultSender<CausedState<TState>> result)
        {
            _state = state;
            _rules = rules;
            _result = result;
        }

        public void Affect(ActionId action, TEffect effect)
        {
            var next = _rules.Apply(_state.Current, effect);
            _result.Broadcast(new CausedState<TState>(next, StateCause.Of(action)));
            _state.Apply(next);
        }
    }
}
