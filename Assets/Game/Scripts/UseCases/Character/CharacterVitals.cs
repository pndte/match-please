using Bw.Entities;

namespace Bw.UseCases.Character
{
    public readonly struct CharacterVitals
    {
        public readonly HealthState Health;
        public readonly CharacterState State;

        public CharacterVitals(HealthState health, CharacterState state)
        {
            Health = health;
            State = state;
        }
    }
}
