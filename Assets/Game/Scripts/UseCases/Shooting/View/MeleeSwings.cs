using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Melee;
using Bw.UseCases.Shooting.Melee.Extensions;
using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.View
{
    public sealed class MeleeSwings : IStateView<WeaponState>, IMeleeSwings
    {
        public ISource<int> Started => _started;

        private readonly Signal<int> _started = new();
        private readonly ISimulationStep _step;
        private readonly MeleeWeaponConfig _config;

        private bool _synchronized;
        private int _swings;

        public MeleeSwings(ISimulationStep step, MeleeWeaponConfig config)
        {
            _step = step;
            _config = config;
        }

        public void Show(WeaponState from, WeaponState to, float progress)
        {
            var swings = Swings(to);
            var started = _synchronized && swings > _swings;
            _swings = swings;
            _synchronized = true;

            if (started)
                _started.Fire(swings);
        }

        private int Swings(WeaponState state) =>
            state.CooldownTicks > _config.SwingTicks(_step) - _config.StrikeTicks(_step) ? state.Shots + 1 : state.Shots;
    }
}
