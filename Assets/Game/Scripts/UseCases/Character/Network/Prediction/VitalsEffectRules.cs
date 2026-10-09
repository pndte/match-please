using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Character.Extensions;

namespace Bw.UseCases.Character.Network.Prediction
{
    public sealed class VitalsEffectRules : IEffectRules<CharacterVitals, float>
    {
        public CharacterVitals Apply(CharacterVitals vitals, float damage) =>
            vitals.State == CharacterState.Dead ? vitals.WithoutChange() : vitals.AfterHit(damage);
    }
}
