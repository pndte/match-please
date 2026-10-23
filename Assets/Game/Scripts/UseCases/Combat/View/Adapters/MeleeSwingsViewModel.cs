using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Melee;
using Bw.UseCases.Combat.Melee.Extensions;
using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.View.Adapters
{
    public sealed class MeleeSwingsViewModel : IStateView<MeleeState>, IMeleeSwingsViewModel
    {
        public ISource<MeleeSwing> Started => _started;
        public IReadonlyProperty<bool> Charging => _charging;
        public IReadonlyProperty<bool> FullyCharged => _fullyCharged;

        private readonly Signal<MeleeSwing> _started = new();
        private readonly ViewableProperty<bool> _charging = new(false);
        private readonly ViewableProperty<bool> _fullyCharged = new(false);
        private readonly ISimulationStep _step;
        private readonly MeleeWeaponConfig _config;

        private bool _synchronized;
        private int _swings;

        public MeleeSwingsViewModel(ISimulationStep step, MeleeWeaponConfig config)
        {
            _step = step;
            _config = config;
        }

        public void Show(MeleeState from, MeleeState to, float progress)
        {
            var swings = to.StrikePending(_config, _step) ? to.Shots + 1 : to.Shots;
            var started = _synchronized && swings > _swings;
            _swings = swings;
            _synchronized = true;
            _charging.Value = to.Phase == MeleePhase.Charging;
            _fullyCharged.Value = to.FullyCharged(_config, _step);

            if (started)
                _started.Fire(new MeleeSwing(swings, to.Phase == MeleePhase.StrongSwing));
        }
    }
}
