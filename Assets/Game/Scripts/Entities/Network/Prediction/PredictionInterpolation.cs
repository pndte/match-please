using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;

namespace Bw.Entities.Network.Prediction
{
    public sealed class PredictionInterpolation<TState> where TState : struct
    {
        private readonly IInterpolationTicks _ticks;
        private readonly IReadonlySimulation<TState> _simulation;
        private readonly IStateView<TState> _view;

        public PredictionInterpolation(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IInterpolationTicks ticks,
            IReadonlySimulation<TState> simulation,
            IStateView<TState> view)
        {
            _ticks = ticks;
            _simulation = simulation;
            _view = view;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(UpdateView));
        }

        private void UpdateView(Unit _) =>
            _view.Show(_simulation.Previous, _simulation.State.Value, _ticks.Progress);
    }
}
