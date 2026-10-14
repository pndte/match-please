using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Impact
{
    public sealed class StrikeImpacts : IStrikeImpacts
    {
        public ISource<StrikeImpact> Bodies => _bodies;
        public ISource<StrikeImpact> Surfaces => _surfaces;

        private readonly Signal<StrikeImpact> _bodies = new();
        private readonly Signal<StrikeImpact> _surfaces = new();
        private readonly IShotTracer _tracer;
        private readonly StrikeImpactConfig _config;

        public StrikeImpacts(
            Lifetime lifetime,
            ISeenShots strikes,
            IShotTracer tracer,
            StrikeImpactConfig config)
        {
            _tracer = tracer;
            _config = config;

            strikes.Seen.Advise(lifetime, Trace);
        }

        private void Trace(float aim)
        {
            var ray = _tracer.Aim(aim);
            var trace = _tracer.Cast(ray);
            if (!trace.Hit) //TODO: попадание видно по локальной трассировке, а урон решает сервер (свой удар предсказан той же трассировкой, но сервер может его отменить) — изредка будет эффект без урона или урон без эффекта
                return;

            var impact = new StrikeImpact(trace.To, ray.Direction);
            if (IsBody(trace.Hit.collider))
                _bodies.Fire(impact);
            else
                _surfaces.Fire(impact);
        }

        private bool IsBody(Collider2D collider) =>
            (_config.BodyLayers.value & (1 << collider.gameObject.layer)) != 0;
    }
}
