using System;
using Bw.Entities.Extensions;

namespace Bw.UseCases.Mortality.Extensions
{
    public static class VitalsExtensions
    {
        public static Vitals AfterHit(this Vitals vitals, float damage)
        {
            if (vitals.State == LifeState.Dead)
                throw new InvalidOperationException("The dead can't be hit.");

            var health = vitals.Health.AfterHit(damage);
            return new Vitals(health, health.Current <= 0f ? LifeState.Dead : LifeState.Alive);
        }

        public static Vitals WithoutChange(this Vitals vitals) =>
            new(vitals.Health.WithoutChange(), vitals.State);

        public static bool IsKillAfter(this Vitals vitals, Vitals previous) =>
            previous.State == LifeState.Alive && vitals.State == LifeState.Dead &&
            (vitals.Health.Change < 0f || previous.Health.Current <= 0f);
    }
}
