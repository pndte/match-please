using Bw.Entities.Simulation;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class SimulationAffectable<TState, TEffect> : IAffectable<TEffect> where TState : struct where TEffect : struct
    {
        private readonly IRestorable<TState> _state;
        private readonly IEffectRules<TState, TEffect> _rules;

        public SimulationAffectable(IRestorable<TState> state, IEffectRules<TState, TEffect> rules)
        {
            _state = state;
            _rules = rules;
        }

        public void Affect(StateCause cause, TEffect effect) =>
            _state.Apply(_rules.Apply(_state.State.Value, effect));
    }
}
