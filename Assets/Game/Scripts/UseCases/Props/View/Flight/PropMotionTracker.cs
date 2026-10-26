using Bw.Entities.Physics;
using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Props.View.Flight
{
    public sealed class PropMotionTracker
    {
        private const float TeleportSpeed = 90f;

        public ISource<PropMotionFrame> Moved => _moved;
        public ISource<PropImpact> Impacted => _impacted;

        private readonly Signal<PropMotionFrame> _moved = new();
        private readonly Signal<PropImpact> _impacted = new();

        private readonly Transform _prop;
        private readonly BodyBox _shape;
        private readonly IWeaponCarryViewModel _carry;
        private readonly PropVfxConfig _config;

        private bool _measured;
        private bool _held;
        private bool _fast;
        private Vector2 _center;
        private Vector2 _holder;
        private Vector2 _velocity;
        private float _sinceImpact;

        public PropMotionTracker(
            Lifetime lifetime,
            BodyBox shape,
            IWeaponCarryViewModel carry,
            PropVfxConfig config)
        {
            _prop = shape.transform;
            _shape = shape;
            _carry = carry;
            _config = config;

            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate, lifetime).Subscribe(_ => Measure(Time.deltaTime));
        }

        private void Measure(float deltaTime)
        {
            var center = (Vector2)_prop.TransformPoint(_shape.Offset);
            var held = _carry.Held.Value;
            var holder = held ? _carry.Holder.Position : center;
            _sinceImpact += deltaTime;
            if (TryGetMotion(center, holder, held, deltaTime, out var velocity, out var swing))
            {
                _fast = swing.magnitude > (_fast ? _config.TrailSpeed * _config.TrailHold : _config.TrailSpeed);
                _moved.Fire(new PropMotionFrame(center, swing, held, _fast));
                if (!held)
                    Collide(center, velocity);
            }
            else
            {
                _fast = false;
                _velocity = Vector2.zero;
            }

            _measured = true;
            _held = held;
            _center = center;
            _holder = holder;
        }

        private bool TryGetMotion(
            Vector2 center,
            Vector2 holder,
            bool held,
            float deltaTime,
            out Vector2 velocity,
            out Vector2 swing)
        {
            velocity = Vector2.zero;
            swing = Vector2.zero;
            if (!_measured || held != _held || deltaTime <= 0f)
                return false;

            velocity = (center - _center) / deltaTime;
            swing = held ? velocity - (holder - _holder) / deltaTime : velocity;
            return swing.magnitude <= TeleportSpeed;
        }

        private void Collide(Vector2 center, Vector2 velocity)
        {
            var change = velocity - _velocity;
            var hit = _velocity.magnitude > _config.ImpactSpeed && change.magnitude > _config.ImpactSpeed && _sinceImpact > _config.ImpactCooldown;
            _velocity = velocity;
            if (!hit)
                return;

            _sinceImpact = 0f;
            var normal = change.normalized;
            var reach = Mathf.Abs(normal.x) * _shape.Size.x / 2f + Mathf.Abs(normal.y) * _shape.Size.y / 2f;
            _impacted.Fire(new PropImpact(center - normal * reach, normal, change.magnitude));
        }
    }
}
