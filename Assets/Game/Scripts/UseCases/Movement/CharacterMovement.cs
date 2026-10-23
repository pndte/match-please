using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public sealed class CharacterMovement : IMovement, ISteppable<MovementInput>
    {
        public IReadonlyProperty<MovementState> State => _state;
        public MovementState Previous { get; private set; }
        public ISource<MovementState> Stepped => _stepped;

        private readonly ViewableProperty<MovementState> _state;
        private readonly Signal<MovementState> _stepped = new();
        private readonly IMovementMotor _motor;

        public CharacterMovement(IMovementMotor motor, IMovementBody body)
        {
            _motor = motor;
            Previous = new MovementState(new BodyState(body.Position, Vector2.zero, 0f, false),
                new JumpState(false, 0, 0));
            _state = new ViewableProperty<MovementState>(Previous);
        }

        public void Apply(MovementState state) =>
            _state.Value = state;

        public void Step(MovementInput input)
        {
            Previous = _state.Value;
            _state.Value = _motor.Step(Previous, input);
            _stepped.Fire(_state.Value);
        }
    }
}
