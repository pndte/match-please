using Bw.Entities;
using Bw.UseCases.Mortality;

namespace Bw.Tests.UseCases.Mortality.Doubles
{
    public static class VitalsOf
    {
        public static Vitals Alive(float health, float change = 0f) =>
            new(new HealthState(health, change), LifeState.Alive);

        public static Vitals Dead(float health = 0f, float change = 0f) =>
            new(new HealthState(health, change), LifeState.Dead);
    }
}
