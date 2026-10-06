using Bw.Entities.Pool;
using Bw.UseCases.Vfx.View.Effects.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;
using Unit = JetBrains.Core.Unit;

namespace Bw.UseCases.Vfx.View.Effects
{
    public sealed class ParticleEffect : MonoBehaviour, IResource<IEffect>, IEffect
    {
        public ISource<Unit> Finished => _finished;

        [SerializeField] private ParticleSystem _particles;
        private readonly Signal<Unit> _finished = new();
        private Lifetime _lifetime;

        public IEffect Facade(Lifetime lifetime)
        {
            _lifetime = lifetime;
            lifetime.OnTermination(() => _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear));
            return this;
        }

        public void Play(Vector2 point, Vector2 direction)
        {
            transform.SetPositionAndRotation(point, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, direction)));
            _particles.Play();
            Observable.EveryUpdate(UnityFrameProvider.Update, _lifetime)
                .Where(_ => !_particles.IsAlive(true))
                .Take(1)
                .Subscribe(_ => _finished.Fire(Unit.Instance));
        }
    }
}
