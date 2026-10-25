using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Abstractions;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Hands.Doubles
{
    public sealed class FakeMovement : IReadonlyMovement
    {
        public IReadonlyProperty<MovementState> State => _state;
        public MovementState Previous => _state.Value;
        public ISource<MovementState> Stepped => _stepped;

        private readonly ViewableProperty<MovementState> _state = new(default);
        private readonly Signal<MovementState> _stepped = new();

        public void Place(Vector2 position) =>
            _state.Value = new MovementState(new BodyState(position, Vector2.zero, 0f, true), default);
    }
}
