using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Mortality;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network.Prediction
{
    public sealed class StrikeDamagePrediction
    {
        private readonly IOutcomePrediction<IReadonlyMortal, Impacts> _outcomePrediction;
        private readonly IControlledCharacters _controlled;

        public StrikeDamagePrediction(
            Lifetime lifetime,
            IStrikes strikes,
            IOutcomePrediction<IReadonlyMortal, Impacts> outcomePrediction,
            IControlledCharacters controlled)
        {
            _outcomePrediction = outcomePrediction;
            _controlled = controlled;

            strikes.Struck.Advise(lifetime, Predict);
        }

        private void Predict(Strike strike)
        {
            if (!IsControlledHere(strike.Actor))
                return;

            for (var index = 0; index < strike.Affected.Count; index++)
                _outcomePrediction.Predict(strike.Tick, strike.Affected[index].Target, strike.Affected[index].Effect);
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
