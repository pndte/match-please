using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Movement.Extensions;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PushEffectRules<TEffect> : IEffectRules<MovementState, TEffect> where TEffect : struct
    {
        private readonly IPushRules<TEffect> _rules;

        public PushEffectRules(IPushRules<TEffect> rules)
        {
            _rules = rules;
        }

        public MovementState Apply(MovementState state, TEffect effect) =>
            state.Pushed(_rules.Impulse(effect));
    }
}
