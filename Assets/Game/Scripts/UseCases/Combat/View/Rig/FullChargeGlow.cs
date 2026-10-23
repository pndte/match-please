using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.View.Impact;
using DG.Tweening;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Combat.View.Rig
{
    public sealed class FullChargeGlow
    {
        private readonly StrikeFeelConfig _config;
        private readonly Transform _visual;
        private readonly SpriteRenderer[] _fists;
        private readonly Color[] _colors;

        private float _glow;

        public FullChargeGlow(
            Lifetime lifetime,
            IMeleeSwingsViewModel swings,
            StrikeFeelConfig config,
            Transform visual,
            SpriteRenderer[] fists)
        {
            _config = config;
            _visual = visual;
            _fists = fists;
            _colors = new Color[fists.Length];

            swings.FullyCharged.WhenTrue(lifetime, Glow);
        }

        private void Glow(Lifetime chargedLifetime)
        {
            for (var index = 0; index < _fists.Length; index++)
                _colors[index] = _fists[index].color;

            DOTween.Kill(this);
            To(1f, _config.FullChargeFlashTime, Ease.OutQuad).OnComplete(Pulse);
            chargedLifetime.OnTermination(Fade);
        }

        private void Pulse() =>
            To(_config.FullChargePulseLow, _config.FullChargePulseTime, Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

        private void Fade()
        {
            DOTween.Kill(this);
            Apply(0f);
        }

        private Tweener To(float glow, float seconds, Ease ease) =>
            DOTween.To(() => _glow, Apply, glow, seconds)
                .SetEase(ease)
                .SetTarget(this)
                .SetLink(_visual.gameObject);

        private void Apply(float glow)
        {
            _glow = glow;
            var tint = Color.Lerp(Color.white, _config.FullChargeTint, glow);
            for (var index = 0; index < _fists.Length; index++)
                _fists[index].color = _colors[index] * tint;
        }
    }
}
