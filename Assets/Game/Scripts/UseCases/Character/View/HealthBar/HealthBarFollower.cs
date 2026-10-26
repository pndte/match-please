using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Character.View.HealthBar
{
    public sealed class HealthBarFollower
    {
        private const float MaxStep = 1f / 120f;

        private readonly HealthBarWidget _widget;
        private readonly Transform _body;
        private readonly UnityEngine.Camera _camera;
        private readonly HealthBarConfig _config;

        private Vector2 _anchor;
        private Vector2 _position;
        private Vector2 _velocity;

        public HealthBarFollower(
            Lifetime lifetime,
            HealthBarWidget widget,
            Transform body,
            UnityEngine.Camera camera,
            HealthBarConfig config)
        {
            _widget = widget;
            _body = body;
            _camera = camera;
            _config = config;
            _anchor = (Vector2)body.position + config.HeadOffset;
            _position = _anchor;

            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate, lifetime).Subscribe(_ => Follow(Time.deltaTime));
        }

        private void Follow(float deltaTime)
        {
            var anchor = (Vector2)_body.position + _config.HeadOffset;
            var anchorVelocity = (anchor - _anchor) / Mathf.Max(deltaTime, MaxStep);
            _anchor = anchor;
            if ((anchor - _position).sqrMagnitude > _config.SnapDistance * _config.SnapDistance)
            {
                _position = anchor;
                _velocity = anchorVelocity;
            }

            var steps = Mathf.Max(1, Mathf.CeilToInt(deltaTime / MaxStep));
            var step = deltaTime / steps;
            var acceleration = Vector2.zero;
            for (var i = 0; i < steps; i++)
            {
                acceleration = (anchor - _position) * _config.Stiffness - (_velocity - anchorVelocity) * _config.Damping;
                _velocity += acceleration * step;
                _position += _velocity * step;
            }

            Tether(anchor, anchorVelocity);
            var screen = _camera.WorldToScreenPoint(_position);
            _widget.Root.position = new Vector3(screen.x, screen.y, 0f);
            _widget.Body.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(acceleration.x * _config.TiltPerAcceleration, -_config.MaxTilt, _config.MaxTilt));
        }

        private void Tether(Vector2 anchor, Vector2 anchorVelocity)
        {
            var lag = _position - anchor;
            if (lag.sqrMagnitude <= _config.MaxLag * _config.MaxLag)
                return;

            var outward = lag.normalized;
            _position = anchor + outward * _config.MaxLag;
            _velocity -= outward * Mathf.Max(0f, Vector2.Dot(_velocity - anchorVelocity, outward));
        }
    }
}
