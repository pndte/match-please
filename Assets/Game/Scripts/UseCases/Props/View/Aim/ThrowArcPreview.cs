using System;
using Bw.UseCases.Props.View.Abstractions;
using Bw.UseCases.Props.View.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Props.View.Aim
{
    public sealed class ThrowArcPreview : IThrowArcPreview
    {
        private readonly Lifetime _lifetime;
        private readonly ParticleSystem _dots;
        private readonly ParticleSystem.Particle[] _particles;

        private Lifetime _showing = Lifetime.Terminated;

        public ThrowArcPreview(Lifetime lifetime, ParticleSystem dots, PropVfxConfig config)
        {
            _lifetime = lifetime;
            _dots = dots;
            _particles = DotsOf(config);
        }

        public void Show(Lifetime lifetime, IReadonlyProperty<ThrowArc> arc)
        {
            if (_showing.IsAlive)
                throw new InvalidOperationException("The throw arc preview shows one arc at a time: another prop is still aimed.");

            _showing = _lifetime.Intersect(lifetime);
            Draw(arc.Value);
            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate, _showing).Subscribe(_ => Draw(arc.Value));
            _showing.OnTermination(() => _dots.Clear());
        }

        private void Draw(ThrowArc arc)
        {
            for (var index = 0; index < _particles.Length; index++)
                _particles[index].position = arc.PointAt(arc.Duration * (index + 1f) / _particles.Length);

            _dots.SetParticles(_particles, _particles.Length);
        }

        private static ParticleSystem.Particle[] DotsOf(PropVfxConfig config)
        {
            var dots = new ParticleSystem.Particle[config.ArcDotCount];
            for (var index = 0; index < dots.Length; index++)
            {
                var share = index / Mathf.Max(1f, dots.Length - 1f);
                dots[index] = new ParticleSystem.Particle
                {
                    startSize = config.ArcDotSize * Mathf.Lerp(1f, config.ArcDotTaper, share),
                    startColor = new Color(1f, 1f, 1f, Mathf.Lerp(1f, config.ArcDotFade, share)),
                    startLifetime = float.MaxValue,
                    remainingLifetime = float.MaxValue
                };
            }

            return dots;
        }
    }
}
