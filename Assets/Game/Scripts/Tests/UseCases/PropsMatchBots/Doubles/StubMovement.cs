using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Abstractions;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.Tests.UseCases.PropsMatchBots.Doubles
{
    public sealed class StubMovement : IReadonlyMovement
    {
        public IReadonlyProperty<MovementState> State => _state;
        public MovementState Previous { get; private set; }
        public ISource<MovementState> Stepped => _stepped;

        private readonly ViewableProperty<MovementState> _state = new(default);
        private readonly Signal<MovementState> _stepped = new();

        public StubMovement(Vector2 position = default)
        {
            Place(position);
        }

        public void Place(Vector2 position, Vector2 velocity = default, bool grounded = true)
        {
            Previous = _state.Value;
            _state.Value = new MovementState(new BodyState(position, velocity, 0f, grounded), default);
            _stepped.Fire(_state.Value);
        }
    }
}
