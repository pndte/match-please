using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class UnheldStateView<TState> : IStateView<TState> where TState : struct
    {
        private readonly IStateView<TState> _view;
        private readonly IWeaponHold _hold;

        public UnheldStateView(IStateView<TState> view, IWeaponHold hold)
        {
            _view = view;
            _hold = hold;
        }

        public void Show(TState from, TState to, float progress)
        {
            if (!_hold.HeldLifetime.Value.IsAlive)
                _view.Show(from, to, progress);
        }
    }
}
