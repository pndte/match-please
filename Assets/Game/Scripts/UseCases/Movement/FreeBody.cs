using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public sealed class FreeBody : ISimulation<BodyState>
    {
        public IReadonlyProperty<BodyState> State => _state;
        public BodyState Previous { get; private set; }
        public ISource<BodyState> Stepped => _stepped;

        private readonly ViewableProperty<BodyState> _state;
        private readonly Signal<BodyState> _stepped = new();
        private readonly IBodyMotor _motor;

        public FreeBody(
            Lifetime lifetime,
            ISource<Unit> steps,
            IBodyMotor motor,
            IMovementBody body)
        {
            _motor = motor;
            Previous = new BodyState(body.Position, Vector2.zero, 0f, false);
            _state = new ViewableProperty<BodyState>(Previous);

            steps.Advise(lifetime, _ => Step());
        }

        public void Apply(BodyState state) =>
            _state.Value = state;

        private void Step()
        {
            Previous = _state.Value;
            _state.Value = _motor.Step(Previous, Drive(Previous), out _);
            _stepped.Fire(_state.Value);
        }

        private static BodyDrive Drive(BodyState state) =>
            new(new Vector2(0f, state.Velocity.y), 1f, false);
    }
}
