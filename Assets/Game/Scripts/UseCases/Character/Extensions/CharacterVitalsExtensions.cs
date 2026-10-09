using System;
using Bw.Entities.Extensions;

namespace Bw.UseCases.Character.Extensions
{
    public static class CharacterVitalsExtensions
    {
        public static CharacterVitals AfterHit(this CharacterVitals vitals, float damage)
        {
            if (vitals.State == CharacterState.Dead)
                throw new InvalidOperationException("A dead character can't be hit.");

            var health = vitals.Health.AfterHit(damage);
            return new CharacterVitals(health, health.Current <= 0f ? CharacterState.Dead : CharacterState.Alive);
        }

        public static CharacterVitals WithoutChange(this CharacterVitals vitals) =>
            new(vitals.Health.WithoutChange(), vitals.State);

        public static bool IsKillAfter(this CharacterVitals vitals, CharacterVitals previous) =>
            previous.State == CharacterState.Alive && vitals.State == CharacterState.Dead &&
            (vitals.Health.Change < 0f || previous.Health.Current <= 0f);
    }
}
