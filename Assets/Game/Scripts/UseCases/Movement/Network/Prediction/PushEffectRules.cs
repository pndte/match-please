using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Extensions;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PushEffectRules<TState, TEffect> : IEffectRules<TState, TEffect>
        where TState : struct, IBodyState<TState>
        where TEffect : struct
    {
        private readonly IPushRules<TEffect> _rules;

        public PushEffectRules(IPushRules<TEffect> rules)
        {
            _rules = rules;
        }

        public TState Apply(TState state, TEffect effect) =>
            state.Pushed(_rules.Impulse(effect));
    }
}
