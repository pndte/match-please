using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Combat.View.Adapters
{
    public sealed class GunViewModel : IStateView<GunState>, IGunViewModel
    {
        public int Capacity => _config.AmmoSettings.Max;
        public IReadonlyProperty<int> Rounds => _rounds;
        public IReadonlyProperty<GunReload> Reload => _reload;

        private readonly ViewableProperty<int> _rounds;
        private readonly ViewableProperty<GunReload> _reload = new(GunReload.None);
        private readonly ISimulationStep _step;
        private readonly ShootingWeaponConfig _config;

        public GunViewModel(ISimulationStep step, ShootingWeaponConfig config)
        {
            _step = step;
            _config = config;
            _rounds = new ViewableProperty<int>(config.AmmoSettings.OnSpawnValue);
        }

        public void Show(GunState from, GunState to, float progress)
        {
            _rounds.Value = to.Ammo;
            _reload.Value = ReloadOf(from, to, progress);
        }

        private GunReload ReloadOf(GunState from, GunState to, float progress)
        {
            var ticksLeft = from.ReloadTicks > 0 ? Mathf.Lerp(from.ReloadTicks, to.ReloadTicks, progress) : to.ReloadTicks;
            if (ticksLeft <= 0f)
                return GunReload.None;

            var ticks = ReloadTicks();
            var empty = _reload.Value.Active ? _reload.Value.Empty : to.Ammo == 0;
            return new GunReload(empty, 1f - ticksLeft / ticks, ticks * _step.Duration, ticksLeft * _step.Duration);
        }

        private float ReloadTicks() =>
            Mathf.Max(1, Mathf.CeilToInt(_config.ReloadTime / _step.Duration));
    }
}
