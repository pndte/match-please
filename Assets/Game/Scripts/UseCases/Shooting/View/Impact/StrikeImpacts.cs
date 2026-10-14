using Bw.UseCases.Shooting.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.View.Impact
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
            if (!trace.Struck) //TODO: попадание видно по локальной трассировке, а урон решает сервер (свой удар предсказан той же трассировкой, но сервер может его отменить) — изредка будет эффект без урона или урон без эффекта
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
