using Bw.UseCases.Shooting.View.Impact.Abstractions;
using DG.Tweening;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;
using Unit = JetBrains.Core.Unit;

namespace Bw.UseCases.Shooting.View.Impact
{
    public sealed class ParticleImpactPlayback : IImpactEffect
    {
        public ISource<Unit> Finished => _finished;

        private readonly Signal<Unit> _finished = new();
        private readonly Lifetime _lifetime;
        private readonly Transform _transform;
        private readonly ParticleSystem _particles;

        public ParticleImpactPlayback(Lifetime lifetime, Transform transform, ParticleSystem particles)
        {
            _lifetime = lifetime;
            _transform = transform;
            _particles = particles;

            lifetime.OnTermination(() => particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear));
        }

        public void Play(Vector2 point, Vector2 direction, float delay)
        {
            _transform.SetPositionAndRotation(point, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, direction)));
            var start = DOVirtual.DelayedCall(delay, Start, false);
            _lifetime.OnTermination(() => start.Kill());
        }

        private void Start()
        {
            _particles.Play();
            Observable.EveryUpdate(UnityFrameProvider.Update, _lifetime)
                .Where(_ => !_particles.IsAlive(true))
                .Take(1)
                .Subscribe(_ => _finished.Fire(Unit.Instance));
        }
    }
}
