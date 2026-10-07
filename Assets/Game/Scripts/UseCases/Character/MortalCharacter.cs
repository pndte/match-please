using System;
using Bw.Entities;
using JetBrains.Collections.Viewable;
using JetBrains.Core;

namespace Bw.UseCases.Character
{
    public sealed class MortalCharacter : ICharacter
    {
        public IReadonlyHealth Health => _health;
        public IReadonlyProperty<CharacterState> State => _state;
        public ISource<Unit> Died => _died;
        public ISource<CharacterVitals> Applied => _applied;

        private readonly IHealth _health;
        private readonly ViewableProperty<CharacterState> _state = new(CharacterState.Alive);
        private readonly Signal<Unit> _died = new();
        private readonly Signal<CharacterVitals> _applied = new();

        public MortalCharacter(IHealth health)
        {
            _health = health;
        }

        public void Apply(CharacterVitals vitals)
        {
            if (_state.Value == CharacterState.Dead && vitals.State == CharacterState.Alive)
                throw new InvalidOperationException("A dead character can't come back to life.");

            var dies = _state.Value == CharacterState.Alive && vitals.State == CharacterState.Dead;

            _health.Apply(vitals.Health);
            _state.Value = vitals.State;

            if (dies && vitals.Health.Change < 0f)
                _died.Fire(Unit.Instance);

            _applied.Fire(vitals);
        }
    }
}
