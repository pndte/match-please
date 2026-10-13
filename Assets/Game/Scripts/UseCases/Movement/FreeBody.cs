using System;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public sealed class FreeBody : ISimulation<BodyState>, ISteppable, IShovable
    {
        public IReadonlyProperty<BodyState> State => _state;
        public BodyState Previous { get; private set; }
        public ISource<BodyState> Stepped => _stepped;

        private readonly ViewableProperty<BodyState> _state;
        private readonly Signal<BodyState> _stepped = new();
        private readonly IBodyMotor _motor;
        private readonly IMovementBody _body;

        private Vector2 _shoved;

        public FreeBody(IBodyMotor motor, IMovementBody body)
        {
            _motor = motor;
            _body = body;
            Previous = new BodyState(body.Position, Vector2.zero, 0f, false);
            _state = new ViewableProperty<BodyState>(Previous);
        }

        public void Apply(BodyState state) =>
            _state.Value = state;

        public float PossibleShove(Vector2 direction, float distance, IMovementCollider shover) =>
            _motor.PossibleShove(_state.Value.Position + _shoved, direction, distance, shover);

        public void Shove(Vector2 direction, float distance, IMovementCollider shover)
        {
            if (distance <= 0f)
                throw new ArgumentOutOfRangeException(nameof(distance), distance, "A shove moves a body forward, so its distance must be positive.");

            var from = _state.Value.Position + _shoved;
            _motor.MakeWay(from, direction, distance, shover);
            _shoved += direction * distance;
            _body.Place(from + direction * distance);
        }

        public void Step()
        {
            Previous = _state.Value;
            _state.Value = _motor.Step(Previous, _shoved);
            _shoved = Vector2.zero;
            _stepped.Fire(_state.Value);
        }
    }
}
