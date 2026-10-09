using Bw.Entities.Simulation;
using Bw.UseCases.Audio.View.Playback.Abstractions;
using Bw.UseCases.Shooting.Weapon;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Audio
{
    public sealed class WeaponReloadSoundsView : IStateView<WeaponState>
    {
        private readonly Lifetime _lifetime;
        private readonly Transform _weapon;
        private readonly ISoundPlayer _player;
        private readonly ISimulationStep _step;
        private readonly ShootingWeaponConfig _weaponConfig;
        private readonly WeaponSoundsConfig _sounds;

        private bool _reloading;
        private bool _empty;
        private float _progress;

        public WeaponReloadSoundsView(
            Lifetime lifetime,
            Transform weapon,
            ISoundPlayer player,
            ISimulationStep step,
            ShootingWeaponConfig weaponConfig,
            WeaponSoundsConfig sounds)
        {
            _lifetime = lifetime;
            _weapon = weapon;
            _player = player;
            _step = step;
            _weaponConfig = weaponConfig;
            _sounds = sounds;
        }

        public void Show(WeaponState from, WeaponState to, float progress)
        {
            var ticksLeft = from.ReloadTicks > 0 && to.ReloadTicks > 0
                ? Mathf.Lerp(from.ReloadTicks, to.ReloadTicks, progress)
                : to.ReloadTicks;
            if (ticksLeft <= 0f)
            {
                _reloading = false;
                return;
            }

            var current = 1f - ticksLeft / ReloadTicks();
            if (!_reloading)
            {
                _reloading = true;
                _empty = to.Ammo == 0;
                _progress = current;
            }

            foreach (var cue in _sounds.Reload)
                if ((!cue.OnlyEmpty || _empty) && _progress < cue.At && current >= cue.At)
                    _player.Play(_lifetime, cue.Sound, _weapon);

            _progress = current;
        }

        private float ReloadTicks() =>
            Mathf.Max(1, Mathf.CeilToInt(_weaponConfig.ReloadTime / _step.Duration));
    }
}
