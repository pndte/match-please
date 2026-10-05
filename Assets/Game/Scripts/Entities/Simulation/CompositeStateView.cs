namespace Bw.Entities.Simulation
{
    public sealed class CompositeStateView<TState> : IStateView<TState> where TState : struct
    {
        private readonly IStateView<TState>[] _views;

        public CompositeStateView(params IStateView<TState>[] views)
        {
            _views = views;
        }

        public void Show(TState from, TState to, float progress)
        {
            foreach (var view in _views)
                view.Show(from, to, progress);
        }
    }
}
