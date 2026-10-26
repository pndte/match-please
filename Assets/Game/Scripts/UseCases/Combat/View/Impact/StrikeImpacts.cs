using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Impact
{
    public sealed class StrikeImpacts : IStrikeImpacts
    {
        public ISource<StrikeImpact> Bodies => _bodies;
        public ISource<StrikeImpact> Surfaces => _surfaces;

        private readonly Signal<StrikeImpact> _bodies = new();
        private readonly Signal<StrikeImpact> _surfaces = new();
        private readonly StrikeImpactConfig _config;

        public StrikeImpacts(Lifetime lifetime, IShotsViewModel strikes, StrikeImpactConfig config)
        {
            _config = config;

            strikes.Seen.Advise(lifetime, See);
        }

        private void See(SeenShot strike)
        {
            var trace = strike.Pellets[0];
            if (trace.End == PelletEnd.Missed) //TODO: пофиксить, что иногда кровь не совпадает с моментом нанесения урона
                return;

            var impact = new StrikeImpact(trace.To, strike.Direction);
            if (IsBody(trace.Layer))
                _bodies.Fire(impact);
            else
                _surfaces.Fire(impact);
        }

        private bool IsBody(int layer) =>
            (_config.BodyLayers.value & (1 << layer)) != 0;
    }
}
