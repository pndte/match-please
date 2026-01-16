using Entities;
using JetBrains.Lifetimes;

namespace UseCases.Network
{
    public class DamageProcessor
    {
        private readonly ICharacter _character;

        public DamageProcessor(ILifetimed lifetimed, ICharacter character)
        {
            _character = character;
            lifetimed.WhenAlive(WhenCharacterAlive);
        }

        private void WhenCharacterAlive(Lifetime lifetime)
        {
            _character.Health.WhenLessOrEquals(lifetime, 0, OnZeroHealth);
        }

        private void OnZeroHealth()
        {
            _character.Die();
        }
    }
}