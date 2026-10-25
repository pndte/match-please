using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Network.Prediction;
using UnityEngine;

namespace Bw.UseCases.Level.Network
{
    public sealed class PlatformTimelineBlendView : IStateView<PlatformState>
    {
        private readonly IStateView<PlatformState> _view;
        private readonly IPredictedBy _predictedBy;
        private readonly PlatformRail _rail;
        private readonly CorrectionDecay _decay;
        private readonly CorrectionSmoothingConfig _config;

        private float _shown;
        private float _offset;
        private bool _predicted;
        private bool _shownBefore;

        public PlatformTimelineBlendView(
            IStateView<PlatformState> view,
            IPredictedBy predictedBy,
            PlatformRail rail,
            INetworkTicks ticks,
            IInterpolationTicks interpolation,
            CorrectionSmoothingConfig config)
        {
            _view = view;
            _predictedBy = predictedBy;
            _rail = rail;
            _decay = new CorrectionDecay(ticks, interpolation, config.SwitchBlendSeconds);
            _config = config;
        }

        public void Show(PlatformState from, PlatformState to, float progress)
        {
            var along = Mathf.Lerp(from.Along, to.Along, progress);
            var predicted = _predictedBy.Me.Value;
            if (_shownBefore && predicted != _predicted)
                Blend(_shown - along);

            _predicted = predicted;
            _shownBefore = true;

            var offset = _offset * _decay.Remaining;
            _shown = along + offset;
            _view.Show(new PlatformState(from.Along + offset), new PlatformState(to.Along + offset), progress);
        }

        private void Blend(float jump)
        {
            _offset = Mathf.Abs(jump) * _rail.Travel.magnitude > _config.SnapDistance ? 0f : jump;
            _decay.Restart();
        }
    }
}
