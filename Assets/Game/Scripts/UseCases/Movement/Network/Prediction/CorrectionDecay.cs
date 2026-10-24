using Bw.Entities.Network.Ticks;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class CorrectionDecay
    {
        public float Remaining => Mathf.SmoothStep(1f, 0f, (float)((ViewTick() - _since) * _ticks.Duration / _seconds));

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

        private double ViewTick() =>
            _ticks.PredictedViewTick(_interpolation);
    }
}
