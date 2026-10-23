using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Combat.Weapon
{
    public sealed class UnheldStateView<TState> : IStateView<TState> where TState : struct
    {
        private readonly IStateView<TState> _view;
        private readonly IWeapon _weapon;
        private readonly IWeaponCarriers _carriers;

        public UnheldStateView(IStateView<TState> view, IWeapon weapon, IWeaponCarriers carriers)
        {
            _view = view;
            _weapon = weapon;
            _carriers = carriers;
        }

        public void Show(TState from, TState to, float progress)
        {
            if (!_carriers.ByWeapon.ContainsKey(_weapon))
                _view.Show(from, to, progress);
        }
    }
}
