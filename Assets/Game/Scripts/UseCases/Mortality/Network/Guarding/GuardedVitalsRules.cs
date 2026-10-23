using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Guarding.Extensions;

namespace Bw.UseCases.Mortality.Network.Guarding
{
    public sealed class GuardedVitalsRules : IEffectRules<Vitals, Impacts>
    {
        private readonly IEffectRules<Vitals, Impacts> _rules;
        private readonly IGuardCover _cover;

        public GuardedVitalsRules(IEffectRules<Vitals, Impacts> rules, IGuardCover cover)
        {
            _rules = rules;
            _cover = cover;
        }

        public Vitals Apply(Vitals vitals, Impacts impacts)
        {
            var uncovered = _cover.Uncovered(impacts);
            return uncovered.Count == 0 ? vitals.WithoutChange() : _rules.Apply(vitals, uncovered);
        }
    }
}
