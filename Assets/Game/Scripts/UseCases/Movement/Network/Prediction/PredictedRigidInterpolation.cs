using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using JetBrains.Lifetimes;
using R3;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PredictedRigidInterpolation
    {
        private readonly IPredictedRigidBody _body;
        private readonly IStateView<RigidState> _view;
        private readonly INetworkTicks _ticks;
        private readonly IInterpolationTicks _interpolation;

        public PredictedRigidInterpolation(
            Lifetime lifetime,
            IPredictedRigidBody body,
            IStateView<RigidState> view,
            INetworkTicks ticks,
            IInterpolationTicks interpolation)
        {
            _body = body;
            _view = view;
            _ticks = ticks;
            _interpolation = interpolation;

            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate, lifetime).Subscribe(UpdateView);
        }

        private void UpdateView(Unit _)
        {
            if (_body.TryGetShown(_ticks.PredictedViewTick(_interpolation), out var shown))
                _view.Show(shown, shown, 0f);
        }
    }
}
