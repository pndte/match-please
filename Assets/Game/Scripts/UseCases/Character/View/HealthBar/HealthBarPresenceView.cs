using Bw.UseCases.Mortality.View.Health;
using DG.Tweening;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.HealthBar
{
    public sealed class HealthBarPresenceView
    {
        private readonly HealthBarWidget _widget;
        private readonly HealthBarConfig _config;

        private bool _alive;

        public HealthBarPresenceView(
            Lifetime lifetime,
            HealthBarWidget widget,
            IHealthViewModel health,
            HealthBarConfig config)
        {
            _widget = widget;
            _config = config;

            health.Level.Advise(lifetime, level => Live(level.Share > 0f));
        }

        private void Live(bool alive)
        {
            if (alive == _alive)
                return;

            _alive = alive;
            DOTween.Kill(_widget.Group);
            _widget.Root.DOKill();
            if (alive)
                Appear();
            else
                Fade();
        }

        private void Appear()
        {
            _widget.Root.localScale = Vector3.zero;
            _widget.Root.DOScale(1f, _config.AppearTime)
                .SetEase(Ease.OutBack, _config.AppearOvershoot)
                .SetLink(_widget.gameObject);
            DOTween.To(() => _widget.Group.alpha, alpha => _widget.Group.alpha = alpha, 1f, _config.AppearTime * 0.5f)
                .SetTarget(_widget.Group)
                .SetLink(_widget.gameObject);
        }

        private void Fade() =>
            DOTween.To(() => _widget.Group.alpha, alpha => _widget.Group.alpha = alpha, 0f, _config.DeathFadeTime)
                .SetDelay(_config.DeathFadeDelay)
                .SetTarget(_widget.Group)
                .SetLink(_widget.gameObject);
    }
}
