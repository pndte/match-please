using Bw.Entities;
using Bw.UseCases.Character;
using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
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
        private readonly IShotTracer _tracer;

        public StrikeImpacts(Lifetime lifetime, ISeenShots strikes, IShotTracer tracer)
        {
            _tracer = tracer;

            strikes.Seen.Advise(lifetime, Trace);
        }

        private void Trace(float aim)
        {
            var ray = _tracer.Aim(aim);
            var trace = _tracer.Cast(ray);
            if (!trace.Hit) //TODO: попадание видно по локальной трассировке, а урон решает сервер (свой удар предсказан той же трассировкой, но сервер может его отменить) — изредка будет эффект без урона или урон без эффекта
                return;

            var impact = new StrikeImpact(trace.To, ray.Direction);
            if (trace.Hit.collider.TryGetComponent<IHolder<IReadonlyCharacter>>(out _))
                _bodies.Fire(impact);
            else
                _surfaces.Fire(impact);
        }
    }
}
