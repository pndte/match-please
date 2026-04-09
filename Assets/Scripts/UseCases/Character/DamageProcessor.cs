using Bw.Entities.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character
{
    public class DamageProcessor
    {
        private readonly ICharacter _character;

        public DamageProcessor(Lifetime lifetime, ICharacter character)
        {
            _character = character;
            character.Lifetime.WhenAlive(lifetime, WhenCharacterAlive);
        }

        private void WhenCharacterAlive(Lifetime lifetime)
        {
            _character.Health.Current.WhenLessOrEquals(lifetime, 0, OnZeroHealth);
        }

        private void OnZeroHealth()
        {
            _character.Die();
        }
    }
}