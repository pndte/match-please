using Bw.Entities;
using DG.Tweening;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.HealthBar
{
    public sealed class HealthBarPresence
    {
        private readonly HealthBarWidget _widget;
        private readonly HealthBarConfig _config;

        private bool _alive;

        public HealthBarPresence(Lifetime lifetime, HealthBarWidget widget, IReadonlyHealth health, HealthBarConfig config)
        {
            _widget = widget;
            _config = config;

            health.State.Advise(lifetime, state => Live(state.Current > 0f));
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
