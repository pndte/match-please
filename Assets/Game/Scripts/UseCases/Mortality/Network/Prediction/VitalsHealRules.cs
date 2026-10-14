using Bw.Entities;
using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Mortality.Extensions;

namespace Bw.UseCases.Mortality.Network.Prediction
{
    public sealed class VitalsHealRules : IEffectRules<Vitals, Heal>
    {
        private readonly IReadonlyHealth _health;

        public VitalsHealRules(IReadonlyHealth health)
        {
            _health = health;
        }

        public Vitals Apply(Vitals vitals, Heal heal) =>
            vitals.State == LifeState.Dead ? vitals.WithoutChange() : vitals.AfterHeal(heal.Amount, _health.Max);
    }
}
