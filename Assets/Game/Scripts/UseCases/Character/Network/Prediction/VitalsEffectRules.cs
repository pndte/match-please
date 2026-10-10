using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Character.Extensions;

namespace Bw.UseCases.Character.Network.Prediction
{
    public sealed class VitalsEffectRules : IEffectRules<CharacterVitals, Hit>
    {
        public CharacterVitals Apply(CharacterVitals vitals, Hit hit) =>
            vitals.State == CharacterState.Dead ? vitals.WithoutChange() : vitals.AfterHit(hit.Damage);
    }
}
