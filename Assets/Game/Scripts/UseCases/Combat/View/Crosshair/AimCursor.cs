using System.Collections.Generic;
using Bw.UseCases.Combat.View.Crosshair.Abstractions;
using DG.Tweening;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.UI;

namespace Bw.UseCases.Combat.View.Crosshair
{
    public sealed class AimCursor : MonoBehaviour, IAimCursor //TODO: maybe decompose
    {
        [SerializeField] private AimCursorConfig _config;
        [SerializeField] private RectTransform _pointer;
        [SerializeField] private RectTransform _press;
        [SerializeField] private RectTransform _kick;

        [Header("Aim")]
        [SerializeField] private Image _ring;
        [SerializeField] private CanvasGroup _ticksGroup;
        [SerializeField] private Image _ticks;
        [SerializeField] private Image _dot;

        [Header("Reload")]
        [SerializeField] private RectTransform _dial;
        [SerializeField] private CanvasGroup _dialGroup;
        [SerializeField] private Image _arc;
        [SerializeField] private Image _notch;
        [SerializeField] private RectTransform _knob;
        [SerializeField] private Image _bullet;

        [Header("Fists")]
        [SerializeField] private CanvasGroup _fistsGroup;
        [SerializeField] private RectTransform _leftFist;
        [SerializeField] private RectTransform _rightFist;
        [SerializeField] private RectTransform _leftJab;
        [SerializeField] private RectTransform _rightJab;

        [Header("Rounds")]
        [SerializeField] private CursorRounds _rounds;

        private readonly List<Image> _notches = new();
        private IReloadTimer _timer = new IdleTimer();
        private Sequence _morph;
        private Sequence _fistsMorph;
        private int _reload;
        private int _fists;
        private bool _pressed;
        private bool _leftJabs;
        private float _twist = 1f;

        public void ShowReload(Lifetime reloadLifetime, IReloadTimer timer)
        {
            var reload = ++_reload;
            _timer = timer;
            PlaceNotches(timer.Seconds);
            _morph.PlayForward();
            _rounds.Cover(reloadLifetime);
            reloadLifetime.OnTermination(() => Finish(reload));
        }

        public void ShowShot()
        {
            if (_fists > 0)
                Jab();
            else
                Kick();
        }

        public void ShowRounds(Lifetime shownLifetime, IReadonlyProperty<int> rounds, int capacity) =>
            _rounds.Show(shownLifetime, rounds, capacity);

        public void ShowFists(Lifetime drawnLifetime)
        {
            if (++_fists == 1)
                _fistsMorph.PlayForward();

            drawnLifetime.OnTermination(() =>
            {
                if (--_fists == 0)
                    _fistsMorph.PlayBackwards();
            });
        }

        private void Awake()
        {
            _notches.Add(_notch);
            _morph = Morph();
            _fistsMorph = FistsMorph();
            Wobble();
        }

        private void OnEnable() =>
            Cursor.visible = false;

        private void OnDisable() =>
            Cursor.visible = true;

        private void LateUpdate()
        {
            _pointer.position = Input.mousePosition; //TODO: new input system
            var pressed = Input.GetMouseButton(0); //TODO: new input system
            if (pressed != _pressed)
            {
                _pressed = pressed;
                Squeeze(pressed);
            }

            var done = 1f - Mathf.Clamp01(_timer.SecondsLeft / _timer.Seconds);
            _arc.fillAmount = done;
            _knob.localRotation = Quaternion.Euler(0f, 0f, -done * 360f);
        }

        private Sequence Morph()
        {
            var time = _config.TransitionTime;
            _dialGroup.alpha = 0f;
            _dial.localScale = Vector3.one * _config.DialGrow;
            _bullet.rectTransform.localScale = Vector3.zero;
            _bullet.color = new Color(1f, 1f, 1f, 0f);

            return DOTween.Sequence()
                .Join(Fade(_ring, 0f, time))
                .Join(_ring.rectTransform.DOScale(_config.RingSwell, time).SetEase(Ease.InOutSine))
                .Join(Fade(_ticks, 0f, time))
                .Join(_ticks.rectTransform.DOScale(_config.TickRetract, time).SetEase(Ease.InBack, _config.Overshoot))
                .Join(_ticks.rectTransform.DOLocalRotate(new Vector3(0f, 0f, _config.TickTwist), time).SetEase(Ease.InOutSine))
                .Join(Fade(_dot, 0f, time))
                .Join(_dot.rectTransform.DOScale(0f, time).SetEase(Ease.InBack, _config.Overshoot))
                .Join(Fade(_dialGroup, 1f, time))
                .Join(_dial.DOScale(1f, time).SetEase(Ease.OutSine))
                .Join(Fade(_bullet, 1f, time))
                .Join(_bullet.rectTransform.DOScale(1f, time).SetEase(Ease.OutBack, _config.Overshoot))
                .SetAutoKill(false)
                .SetUpdate(true)
                .SetLink(gameObject)
                .Pause();
        }

        private Sequence FistsMorph()
        {
            var time = _config.FistsTime;
            _fistsGroup.alpha = 0f;
            _leftFist.anchoredPosition = new Vector2(-_config.FistsSlide, 0f);
            _rightFist.anchoredPosition = new Vector2(_config.FistsSlide, 0f);
            _leftFist.localScale = new Vector3(_config.FistsGrow, _config.FistsGrow, 1f);
            _rightFist.localScale = new Vector3(-_config.FistsGrow, _config.FistsGrow, 1f);

            return DOTween.Sequence()
                .Join(Fade(_ticksGroup, 0f, time * 0.7f))
                .Join(_ticksGroup.transform.DOScale(_config.TickRetract, time).SetEase(Ease.InBack, _config.Overshoot))
                .Join(Fade(_fistsGroup, 1f, time * 0.6f))
                .Join(Slide(_leftFist, time))
                .Join(Slide(_rightFist, time))
                .Join(_leftFist.DOScale(new Vector3(1f, 1f, 1f), time).SetEase(Ease.OutBack, _config.Overshoot))
                .Join(_rightFist.DOScale(new Vector3(-1f, 1f, 1f), time).SetEase(Ease.OutBack, _config.Overshoot))
                .SetAutoKill(false)
                .SetUpdate(true)
                .SetLink(gameObject)
                .Pause();
        }

        private void Wobble()
        {
            _bullet.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -_config.WobbleAngle);
            _bullet.rectTransform.DOLocalRotate(new Vector3(0f, 0f, _config.WobbleAngle), _config.WobbleTime)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        private void Kick()
        {
            _twist = -_twist;
            _kick.DOComplete();
            _kick.DOPunchScale(Vector3.one * _config.KickScale, _config.KickTime, 1, 0f)
                .SetUpdate(true)
                .SetLink(gameObject);
            _kick.DOPunchRotation(new Vector3(0f, 0f, _twist * _config.KickAngle), _config.KickTime, 2, 0.5f)
                .SetUpdate(true)
                .SetLink(gameObject);
            _kick.DOShakePosition(_config.KickTime, new Vector3(_config.KickShake, _config.KickShake, 0f), _config.KickVibrato)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        private void Jab()
        {
            _leftJabs = !_leftJabs;
            var fist = _leftJabs ? _leftJab : _rightJab;
            fist.DOComplete();
            fist.DOPunchPosition(new Vector3(_config.JabReach, 0f, 0f), _config.JabTime, 1, 0f)
                .SetUpdate(true)
                .SetLink(gameObject);
            fist.DOPunchScale(Vector3.one * _config.JabScale, _config.JabTime, 1, 0f)
                .SetUpdate(true)
                .SetLink(gameObject);
            _kick.DOComplete();
            _kick.DOPunchScale(Vector3.one * _config.JabKick, _config.JabTime, 1, 0f)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        private void Finish(int reload)
        {
            if (reload != _reload)
                return;

            _morph.PlayBackwards();
            if (_timer.SecondsLeft <= 0f)
                Punch();
        }

        private void Squeeze(bool pressed)
        {
            _press.DOKill();
            var squeeze = pressed
                ? _press.DOScale(_config.PressScale, _config.PressTime).SetEase(Ease.OutQuad)
                : _press.DOScale(1f, _config.ReleaseTime).SetEase(Ease.OutBack, _config.ReleaseOvershoot);
            squeeze.SetUpdate(true).SetLink(gameObject);
        }

        private void Punch()
        {
            _pointer.DOComplete();
            _pointer.DOPunchScale(Vector3.one * (_config.PunchScale - 1f), _config.PunchTime, 1, 0f)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        private void PlaceNotches(float seconds)
        {
            var count = Mathf.Max(1, Mathf.CeilToInt(seconds - 0.001f));
            while (_notches.Count < count)
                _notches.Add(Instantiate(_notch, _notch.transform.parent));

            for (var i = 0; i < _notches.Count; i++)
            {
                _notches[i].gameObject.SetActive(i < count);
                _notches[i].rectTransform.localRotation = Quaternion.Euler(0f, 0f, -(1f - i / seconds) * 360f);
            }
        }

        private Tween Slide(RectTransform fist, float time) =>
            DOTween.To(() => fist.anchoredPosition, position => fist.anchoredPosition = position, Vector2.zero, time)
                .SetEase(Ease.OutBack, _config.Overshoot);

        private static Tween Fade(Image image, float alpha, float time) =>
            DOTween.ToAlpha(() => image.color, color => image.color = color, alpha, time);

        private static Tween Fade(CanvasGroup group, float alpha, float time) =>
            DOTween.To(() => group.alpha, value => group.alpha = value, alpha, time);

        private sealed class IdleTimer : IReloadTimer
        {
            public float Seconds => 1f;
            public float SecondsLeft => 0f;
        }
    }
}
