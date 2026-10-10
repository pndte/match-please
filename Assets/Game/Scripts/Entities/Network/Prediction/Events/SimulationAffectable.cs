using Bw.Entities.Simulation;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class SimulationAffectable<TState, TEffect> : IAffectable<TEffect> where TState : struct where TEffect : struct
    {
        private readonly ISimulation<TState> _simulation;
        private readonly IEffectRules<TState, TEffect> _rules;

        public SimulationAffectable(ISimulation<TState> simulation, IEffectRules<TState, TEffect> rules)
        {
            _simulation = simulation;
            _rules = rules;
        }

        public void Affect(ActionId action, TEffect effect) =>
            _simulation.Apply(_rules.Apply(_simulation.State.Value, effect));
    }
}
