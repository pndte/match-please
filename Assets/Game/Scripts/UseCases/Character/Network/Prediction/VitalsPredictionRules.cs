using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Events;

namespace Bw.UseCases.Character.Network.Prediction
{
    public sealed class VitalsPredictionRules : IPredictionRules<CharacterVitals, Hit>
    {
        public CharacterVitals Predict(CharacterVitals vitals, Hit hit) =>
            new(vitals.Health.AfterHit(hit.Damage), vitals.State);

        public CharacterVitals Present(CharacterVitals shown, CharacterVitals next, ChangeCause<CharacterVitals, Hit> cause)
        {
            var shift = next.Health.Current - shown.Health.Current;

            var change = cause.Switch(shift,
                predicted: static (shift, _) => Math.Min(0f, shift),
                authoritative: static (shift, state) =>
                    state.Health.Change < 0f ? Math.Min(0f, shift) : state.Health.Change > 0f ? Math.Max(0f, shift) : 0f,
                correction: static _ => 0f);

            return new CharacterVitals(new HealthState(next.Health.Current, change), next.State);
        }
    }
}
