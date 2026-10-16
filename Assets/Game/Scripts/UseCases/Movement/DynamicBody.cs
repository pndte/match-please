using System;
using Bw.Entities.Extensions;
using Bw.Entities.Physics;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement
{
    public sealed class DynamicBody : ISimulation<RigidState>, IRestartable<RigidState>, IShovable
    {
        public IReadonlyProperty<RigidState> State => _state;
        public RigidState Previous { get; private set; }
        public ISource<RigidState> Stepped => _stepped;

        private readonly ViewableProperty<RigidState> _state;
        private readonly Signal<RigidState> _stepped = new();
        private readonly PhysicsBody _body;

        public DynamicBody(Lifetime lifetime, PhysicsBody body, IPhysicsSteps steps)
        {
            _body = body;
            Previous = body.State();
            _state = new ViewableProperty<RigidState>(Previous);

            steps.Stepped.Advise(lifetime, _ => Step());
        }

        public void Apply(RigidState state)
        {
            _body.ApplyRigidState(state);
            _state.Value = state;
        }

        public void Restart(RigidState state)
        {
            _body.enabled = true;
            _body.ApplyRigidState(state);
            Previous = state;
            _state.Value = state;
            _stepped.Fire(state);
        }

        public float RetreatSpeed(Vector2 direction) =>
            Mathf.Max(0f, Vector2.Dot(_body.linearVelocity, direction));

        public void Shove(Vector2 force)
        {
            if (!float.IsFinite(force.x) || !float.IsFinite(force.y))
                throw new ArgumentOutOfRangeException(nameof(force), force, "A shove pushes with a finite force.");

            _body.ApplyForce(force, _body.worldCenterOfMass, true);
        }

        private void Step()
        {
            if (_body.TryGetLaunchSpeed(out var speed))
                _body.linearVelocity = _body.linearVelocity.WithY(speed);

            Previous = _state.Value;
            _state.Value = _body.State();
            _stepped.Fire(_state.Value);
        }
    }
}
