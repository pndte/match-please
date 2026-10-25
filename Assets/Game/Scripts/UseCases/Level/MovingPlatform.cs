using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Level.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Level
{
    public sealed class MovingPlatform : IReadonlySimulation<PlatformState>, IRestorable<PlatformState>, IMovingPlatform
    {
        public IReadonlyProperty<PlatformState> State => _state;
        public PlatformState Previous { get; private set; }
        public ISource<PlatformState> Stepped => _stepped;
        public Rect Area => new(_rail.PositionAt(_state.Value.Along) + _shape.position, _shape.size);
        public Vector2 Shift => _rail.PositionAt(_state.Value.Along) - _rail.PositionAt(Previous.Along);

        private readonly ViewableProperty<PlatformState> _state;
        private readonly Signal<PlatformState> _stepped = new();
        private readonly PhysicsBody _body;
        private readonly Rect _shape;
        private readonly PlatformRail _rail;
        private readonly ISimulationStep _step;

        public MovingPlatform(
            Lifetime lifetime,
            PhysicsBody body,
            BodyBox box,
            PlatformRail rail,
            ISimulationStep step,
            IPhysicsSteps steps)
        {
            _body = body;
            _shape = ShapeOf(box);
            _rail = rail;
            _step = step;
            Previous = new PlatformState(rail.AlongAt(body.position));
            _state = new ViewableProperty<PlatformState>(Previous);

            steps.Stepped.Advise(lifetime, _ => Step());
        }

        public void DriveTo(float along) =>
            _body.linearVelocity = (_rail.PositionAt(along) - _body.position) / _step.Duration;

        public void Apply(PlatformState state)
        {
            _body.position = _rail.PositionAt(state.Along);
            _state.Value = state;
        }

        private void Step()
        {
            Previous = _state.Value;
            _state.Value = new PlatformState(_rail.AlongAt(_body.position));
            _stepped.Fire(_state.Value);
        }

        private static Rect ShapeOf(BodyBox box)
        {
            var size = box.WorldSize();
            return new Rect(box.WorldOffset() - size / 2f, size);
        }
    }
}
