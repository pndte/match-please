using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public sealed class CharacterMovement : IMovement
    {
        public IReadonlyProperty<MovementState> State => _state;
        public MovementState Previous { get; private set; }
        public ISource<MovementState> Stepped => _stepped;

        private readonly ViewableProperty<MovementState> _state;
        private readonly Signal<MovementState> _stepped = new();
        private readonly IMovementMotor _motor;

        public CharacterMovement(
            Lifetime lifetime,
            ISource<MovementInput> inputs,
            IMovementMotor motor,
            IMovementBody body)
        {
            _motor = motor;
            Previous = new MovementState(body.Position, Vector2.zero, false);
            _state = new ViewableProperty<MovementState>(Previous);

            inputs.Advise(lifetime, Step);
        }

        public void Apply(MovementState state) =>
            _state.Value = state;

        private void Step(MovementInput input)
        {
            Previous = _state.Value;
            _state.Value = _motor.Step(Previous, input);
            _stepped.Fire(_state.Value);
        }
    }
}
