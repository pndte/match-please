using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Mortality.Extensions;

namespace Bw.UseCases.Mortality.Network.Prediction
{
    public sealed class VitalsEffectRules : IEffectRules<Vitals, Impacts>
    {
        public Vitals Apply(Vitals vitals, Impacts impacts) =>
            vitals.State == LifeState.Dead ? vitals.WithoutChange() : vitals.AfterHit(impacts.Damage());
    }
}
