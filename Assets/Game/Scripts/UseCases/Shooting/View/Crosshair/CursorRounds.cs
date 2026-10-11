using System;
using System.Globalization;
using DG.Tweening;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.UI;

namespace Bw.UseCases.Shooting.View.Crosshair
{
    public sealed class CursorRounds : MonoBehaviour
    {
        [SerializeField] private AimCursorConfig _config;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _pulse;
        [SerializeField] private RectTransform _number;
        [SerializeField] private Image[] _digits;
        [SerializeField] private Sprite[] _plain;
        [SerializeField] private Sprite[] _alarm;

        private int _shown;
        private int _covered;
        private int _rounds;
        private bool _visible;
        private bool _low;

        public void Show(Lifetime shownLifetime, IReadonlyProperty<int> rounds, int capacity)
        {
            _shown++;
            _rounds = -1;
            Refresh();
            rounds.Advise(shownLifetime, count => Count(count, capacity));
            shownLifetime.OnTermination(() =>
            {
                _shown--;
                Refresh();
            });
        }

        public void Cover(Lifetime coveredLifetime)
        {
            _covered++;
            Refresh();
            coveredLifetime.OnTermination(() =>
            {
                _covered--;
                Refresh();
            });
        }

        private void Awake()
        {
            _group.alpha = 0f;
            transform.localScale = Vector3.one * _config.RoundsTuck;
        }

        private void Refresh()
        {
            var visible = _shown > 0 && _covered == 0;
            if (visible == _visible)
                return;

            _visible = visible;
            var seconds = visible ? _config.RoundsShowTime : _config.RoundsHideTime;
            DOTween.Kill(_group);
            DOTween.To(() => _group.alpha, alpha => _group.alpha = alpha, visible ? 1f : 0f, seconds)
                .SetTarget(_group)
                .SetUpdate(true)
                .SetLink(gameObject);
            transform.DOKill();
            transform.DOScale(visible ? 1f : _config.RoundsTuck, seconds)
                .SetEase(visible ? Ease.OutBack : Ease.InBack, _config.Overshoot)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        private void Count(int count, int capacity)
        {
            var low = count <= Mathf.Max(1, Mathf.FloorToInt(capacity * _config.RoundsLowShare));
            Write(count, low ? _alarm : _plain);
            if (count < _rounds)
                Tick();
            else if (count > _rounds)
                Pop();

            _rounds = count;
            Pulse(low);
        }

        private void Write(int count, Sprite[] sprites)
        {
            var text = count.ToString(CultureInfo.InvariantCulture);
            if (text.Length > _digits.Length)
                throw new ArgumentOutOfRangeException(nameof(count), count, $"The rounds counter shows at most {_digits.Length.ToString(CultureInfo.InvariantCulture)} digits.");

            for (var i = 0; i < _digits.Length; i++)
            {
                var used = i < text.Length;
                _digits[i].enabled = used;
                if (used)
                    _digits[i].sprite = sprites[text[i] - '0'];
            }
        }

        private void Tick()
        {
            _number.DOComplete();
            _number.DOPunchScale(Vector3.one * _config.RoundsTickScale, _config.RoundsTickTime, 1, 0f)
                .SetUpdate(true)
                .SetLink(gameObject);
            _number.DOPunchPosition(new Vector3(0f, -_config.RoundsTickDrop, 0f), _config.RoundsTickTime, 1, 0f)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        private void Pop()
        {
            _number.DOComplete();
            _number.localScale = Vector3.one * _config.RoundsTuck;
            _number.DOScale(1f, _config.RoundsPopTime)
                .SetEase(Ease.OutBack, _config.Overshoot)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        private void Pulse(bool low)
        {
            if (low == _low)
                return;

            _low = low;
            _pulse.DOKill();
            _pulse.localScale = Vector3.one;
            if (low)
                _pulse.DOScale(_config.RoundsPulseScale, _config.RoundsPulseTime)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true)
                    .SetLink(gameObject);
        }
    }
}
