using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Ticks;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PredictedPush<TEffect> : IPredictionTarget<TEffect>, IPredictedPush where TEffect : struct
    {
        public Vector2 Offset => OffsetAt(_ticks.PredictedViewTick(_interpolation), _interpolation.InterpolationTick);

        private readonly List<PredictedEffect<TEffect>> _pushes = new();
        private readonly List<Fade> _fades = new();

        private readonly PushSimulation<TEffect> _simulation;
        private readonly INetworkTicks _ticks;
        private readonly IInterpolationTicks _interpolation;
        private readonly PushPredictionConfig _config;

        public PredictedPush(
            Lifetime lifetime,
            PushSimulation<TEffect> simulation,
            INetworkTicks ticks,
            IInterpolationTicks interpolation,
            PushPredictionConfig config)
        {
            _simulation = simulation;
            _ticks = ticks;
            _interpolation = interpolation;
            _config = config;

            ticks.Ticked(TickPhase.Default).Advise(lifetime, _ => Advance());
        }

        public void Show(PredictedEffect<TEffect> effect)
        {
            _pushes.Add(effect);
            Simulate();
        }

        public void Confirm(ActionId action, TEffect actual)
        {
            if (!Waiting(action))
                return;

            Withdraw(action);
            _pushes.Add(new PredictedEffect<TEffect>(action, actual));
            Simulate();
        }

        public void Reject(ActionId action)
        {
            if (!Waiting(action))
                return;

            var shown = _ticks.PredictedViewTick(_interpolation);
            var interpolated = _interpolation.InterpolationTick;
            var before = PushedOffset(shown, interpolated);
            Withdraw(action);
            if (_pushes.Count > 0)
                Simulate();

            _fades.Add(new Fade(before - PushedOffset(shown, interpolated), shown));
        }

        private void Simulate() =>
            _simulation.Simulate(_pushes, _ticks.Current);

        private void Advance()
        {
            if (_pushes.Count > 0)
                _simulation.Continue(_ticks.Current);

            if (_pushes.Count > 0 && _interpolation.InterpolationTick >= _simulation.SettledTick)
                _pushes.Clear();

            var shown = _ticks.PredictedViewTick(_interpolation);
            for (var index = _fades.Count - 1; index >= 0; index--)
                if (Remaining(_fades[index], shown) <= 0f)
                    _fades.RemoveAt(index);
        }

        private Vector2 OffsetAt(double shown, double interpolated)
        {
            var offset = PushedOffset(shown, interpolated);
            foreach (var fade in _fades)
                offset += fade.Offset * Remaining(fade, shown);

            return offset;
        }

        private Vector2 PushedOffset(double shown, double interpolated) =>
            _pushes.Count == 0 ? Vector2.zero : _simulation.OffsetAt(shown) - _simulation.OffsetAt(interpolated);

        private float Remaining(Fade fade, double shown) =>
            Mathf.Clamp01(1f - (float)((shown - fade.Since) * _ticks.Duration / _config.RejectedFadeSeconds));

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

        private readonly struct Fade
        {
            public readonly Vector2 Offset;
            public readonly double Since;

            public Fade(Vector2 offset, double since)
            {
                Offset = offset;
                Since = since;
            }
        }
    }
}
