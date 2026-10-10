using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Movement.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PredictedPush<TEffect> : IPredictionTarget<TEffect>, IPredictedPush where TEffect : struct
    {
        public Vector2 Offset => new(OffsetAt(ShownTick(), _interpolation.InterpolationTick), 0f);

        private readonly List<Push> _pushes = new();
        private readonly List<Fade> _fades = new();
        private readonly IPushRules<TEffect> _rules;
        private readonly INetworkTicks _ticks;
        private readonly IInterpolationTicks _interpolation;
        private readonly MovementConfig _movement;
        private readonly PushPredictionConfig _config;

        public PredictedPush(
            Lifetime lifetime,
            IPushRules<TEffect> rules,
            INetworkTicks ticks,
            IInterpolationTicks interpolation,
            MovementConfig movement,
            PushPredictionConfig config)
        {
            _rules = rules;
            _ticks = ticks;
            _interpolation = interpolation;
            _movement = movement;
            _config = config;

            ticks.Ticked(TickPhase.Default).Advise(lifetime, _ => DropFinished());
        }

        public void Show(PredictedEffect<TEffect> effect) =>
            _pushes.Add(new Push(effect.Action, HorizontalImpulse(effect.Value)));

        public void Confirm(ActionId action, TEffect actual)
        {
            if (!Waiting(action))
                return;

            Withdraw(action);
            _pushes.Add(new Push(action, HorizontalImpulse(actual)));
        }

        public void Reject(ActionId action)
        {
            if (!Waiting(action))
                return;

            var shown = ShownTick();
            var interpolated = _interpolation.InterpolationTick;
            var offset = 0f;
            foreach (var push in _pushes)
                if (push.Action.Equals(action))
                    offset += OffsetOf(push, shown, interpolated);

            Withdraw(action);
            _fades.Add(new Fade(offset, shown));
        }

        private float HorizontalImpulse(TEffect effect) => //TODO: вертикальная часть толчка уходит в скорость по Y и зависит от земли под ногами — стрелок видит её только по снимкам, без предсказания
            _rules.Impulse(effect).x;

        private float OffsetAt(double shown, double interpolated)
        {
            var offset = 0f;
            foreach (var push in _pushes)
                offset += OffsetOf(push, shown, interpolated);

            foreach (var fade in _fades)
                offset += fade.Offset * Remaining(fade, shown);

            return offset;
        }

        private float OffsetOf(Push push, double shown, double interpolated) => //TODO: наложенные толчки сервер гасит как одну скорость, а здесь каждый гаснет отдельно — пока снимки не догнали, сдвиг от двух толчков подряд показывается короче настоящего; и стену на пути здесь не видно: тело заходит в неё, пока снимки не догонят
            _movement.PushDistance(push.Impulse, shown - push.Action.Tick, _ticks)
            - _movement.PushDistance(push.Impulse, interpolated - push.Action.Tick, _ticks);

        private float Remaining(Fade fade, double shown) =>
            Mathf.Clamp01(1f - (float)((shown - fade.Since) * _ticks.Duration / _config.RejectedFadeSeconds));

        private void DropFinished()
        {
            var interpolated = _interpolation.InterpolationTick;
            for (var index = _pushes.Count - 1; index >= 0; index--)
                if (interpolated - _pushes[index].Action.Tick >= _movement.PushTicks(_pushes[index].Impulse, _ticks))
                    _pushes.RemoveAt(index);

            var shown = ShownTick();
            for (var index = _fades.Count - 1; index >= 0; index--)
                if (Remaining(_fades[index], shown) <= 0f)
                    _fades.RemoveAt(index);
        }

        private double ShownTick() =>
            _ticks.Current - 1 + (double)_interpolation.Progress;

        private bool Waiting(ActionId action)
        {
            foreach (var push in _pushes)
                if (push.Action.Equals(action))
                    return true;

            return false;
        }

        private void Withdraw(ActionId action)
        {
            for (var index = _pushes.Count - 1; index >= 0; index--)
                if (_pushes[index].Action.Equals(action))
                    _pushes.RemoveAt(index);
        }

        private readonly struct Push
        {
            public readonly ActionId Action;
            public readonly float Impulse;

            public Push(ActionId action, float impulse)
            {
                Action = action;
                Impulse = impulse;
            }
        }

        private readonly struct Fade
        {
            public readonly float Offset;
            public readonly double Since;

            public Fade(float offset, double since)
            {
                Offset = offset;
                Since = since;
            }
        }
    }
}
