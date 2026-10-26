using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Guarding.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network.Prediction
{
    public sealed class StrikeDamagePredictor
    {
        private readonly IOutcomePredictor<IReadonlyMortal, Impacts> _outcomePredictor;
        private readonly IControlledCharacters _controlled;
        private readonly IReadonlyGuardCovers _covers;

        public StrikeDamagePredictor(
            Lifetime lifetime,
            IStrikes strikes,
            IOutcomePredictor<IReadonlyMortal, Impacts> outcomePredictor,
            IControlledCharacters controlled,
            IReadonlyGuardCovers covers)
        {
            _outcomePredictor = outcomePredictor;
            _controlled = controlled;
            _covers = covers;

            strikes.Struck.Advise(lifetime, Predict);
        }

        private void Predict(Strike strike)
        {
            if (!IsControlledHere(strike.Actor))
                return;

            for (var index = 0; index < strike.Affected.Count; index++)
                Predict(strike.Tick, strike.Affected[index].Target, strike.Affected[index].Effect);
        }

        private void Predict(int tick, IReadonlyMortal target, Impacts impacts)
        {
            var uncovered = _covers.CoverOf(target).Uncovered(impacts);
            if (uncovered.Count > 0)
                _outcomePredictor.Predict(tick, target, uncovered);
        }

        private bool IsControlledHere(IReadonlyCharacter actor)
        {
            for (var index = 0; index < _controlled.Count; index++)
                if (_controlled[index] == actor)
                    return true;

            return false;
        }
    }
}
