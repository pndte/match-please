using Bw.Entities.Network.Ticks;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class CorrectionDecay
    {
        public float Remaining => Mathf.SmoothStep(1f, 0f, Elapsed());
        public float Rate
        {
            get
            {
                var elapsed = Elapsed();
                return elapsed > 0f && elapsed < 1f ? -6f * elapsed * (1f - elapsed) / _seconds : 0f;
            }
        }

        private readonly INetworkTicks _ticks;
        private readonly IInterpolationTicks _interpolation;
        private readonly float _seconds;

        private double _since;

        public CorrectionDecay(INetworkTicks ticks, IInterpolationTicks interpolation, float seconds)
        {
            _ticks = ticks;
            _interpolation = interpolation;
            _seconds = seconds;
        }

        public void Restart() =>
            _since = ViewTick();

        private float Elapsed() =>
            (float)((ViewTick() - _since) * _ticks.Duration / _seconds);

        private double ViewTick() =>
            _ticks.PredictedViewTick(_interpolation);
    }
}
