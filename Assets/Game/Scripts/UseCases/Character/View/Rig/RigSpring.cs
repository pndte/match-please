using UnityEngine;

namespace Bw.UseCases.Character.View.Rig
{
    public sealed class RigSpring
    {
        private const float MaxStep = 1f / 240f;

        public Vector2 Offset => _position - _target;
        public Vector2 Target => _target;

        private Vector2 _position;
        private Vector2 _velocity;
        private Vector2 _target;

        public RigSpring(Vector2 start)
        {
            _position = start;
            _target = start;
        }

        public void Step(
            Vector2 target,
            float frequency,
            float damping,
            float deltaTime)
        {
            var stiffness = (2f * Mathf.PI * frequency) * (2f * Mathf.PI * frequency);
            var drag = 2f * damping * Mathf.Sqrt(stiffness);
            var steps = Mathf.CeilToInt(deltaTime / MaxStep);
            for (var i = 1; i <= steps; i++)
            {
                var step = deltaTime / steps;
                var followed = Vector2.Lerp(_target, target, (float)i / steps);
                var targetVelocity = (target - _target) / deltaTime;
                _velocity += (stiffness * (followed - _position) + drag * (targetVelocity - _velocity)) * step;
                _position += _velocity * step;
            }

            _target = target;
        }
    }
}
