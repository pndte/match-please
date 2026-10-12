using UnityEngine;

namespace Bw.UseCases.Vfx.View.Effects
{
    public sealed class DebrisSettling : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particles;
        [SerializeField, Min(0f)] private float _restSpeed = 0.5f;
        [SerializeField, Min(0f)] private float _tipOverSpeed = 600f;
        private ParticleSystem.Particle[] _pieces;

        private void Awake() =>
            _pieces = new ParticleSystem.Particle[_particles.main.maxParticles];

        private void LateUpdate() =>
            Settle(Time.deltaTime);

        private void Settle(float deltaTime)
        {
            var count = _particles.GetParticles(_pieces);
            var turn = _tipOverSpeed * deltaTime;
            var settled = false;
            for (var index = 0; index < count; index++)
            {
                if (_pieces[index].totalVelocity.sqrMagnitude > _restSpeed * _restSpeed)
                    continue;

                var rotation = _pieces[index].rotation;
                _pieces[index].rotation = Mathf.MoveTowards(rotation, Mathf.Round(rotation / 180f) * 180f, turn);
                settled = true;
            }

            if (settled)
                _particles.SetParticles(_pieces, count);
        }
    }
}
