using Bw.Entities;

namespace Bw.UseCases.Mortality
{
    public readonly struct Vitals
    {
        public readonly HealthState Health;
        public readonly LifeState State;

        public Vitals(HealthState health, LifeState state)
        {
            Health = health;
            State = state;
        }
    }
}
