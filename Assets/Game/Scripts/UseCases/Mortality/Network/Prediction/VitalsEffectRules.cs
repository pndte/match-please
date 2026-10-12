using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Mortality.Extensions;

namespace Bw.UseCases.Mortality.Network.Prediction
{
    public sealed class VitalsEffectRules : IEffectRules<Vitals, Hit>
    {
        public Vitals Apply(Vitals vitals, Hit hit) =>
            vitals.State == LifeState.Dead ? vitals.WithoutChange() : vitals.AfterHit(hit.Damage);
    }
}
