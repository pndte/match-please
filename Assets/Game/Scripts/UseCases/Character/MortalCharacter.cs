using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Character
{
    public sealed class MortalCharacter : ICharacter
    {
        public IReadonlyHealth Health => _health;
        public IReadonlyProperty<CharacterState> State => _state;
        public ISource<CharacterVitals> Applied => _applied;
        public CharacterVitals Current => new(_health.State.Value.WithoutChange(), _state.Value);

        private readonly IHealth _health;
        private readonly ViewableProperty<CharacterState> _state = new(CharacterState.Alive);
        private readonly Signal<CharacterVitals> _applied = new();

        public MortalCharacter(IHealth health)
        {
            _health = health;
        }

        public void Apply(CharacterVitals vitals)
        {
            if (_state.Value == CharacterState.Dead && vitals.State == CharacterState.Alive)
                throw new InvalidOperationException("A dead character can't come back to life.");

            _health.Apply(vitals.Health);
            _state.Value = vitals.State;
            _applied.Fire(vitals);
        }
    }
}
