using Bw.Entities.Simulation;
using Bw.UseCases.Audio.View.Playback;
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
        private readonly WeaponSoundsConfig _config;

        private float _secondsLeft;
        private bool _empty;

        public WeaponReloadSoundsView(
            Lifetime lifetime,
            Transform weapon,
            ISoundPlayer player,
            ISimulationStep step,
            WeaponSoundsConfig config)
        {
            _lifetime = lifetime;
            _weapon = weapon;
            _player = player;
            _step = step;
            _config = config;
        }

        public void Show(WeaponState from, WeaponState to, float progress)
        {
            var ticksLeft = from.ReloadTicks > 0 && to.ReloadTicks > 0
                ? Mathf.Lerp(from.ReloadTicks, to.ReloadTicks, progress)
                : to.ReloadTicks;
            var secondsLeft = ticksLeft * _step.Duration;
            if (secondsLeft <= 0f)
            {
                _secondsLeft = 0f;
                return;
            }

            if (_secondsLeft <= 0f)
                Start(to);

            if (Reaches(secondsLeft, _config.MagazineInBeforeEnd))
                Play(_config.MagazineIn);
            if (_empty && Reaches(secondsLeft, _config.BoltRackBeforeEnd))
                Play(_config.BoltRack);

            _secondsLeft = secondsLeft;
        }

        private void Start(WeaponState state)
        {
            _empty = state.Ammo == 0;
            _secondsLeft = float.PositiveInfinity;
            Play(_config.MagazineOut);
        }

        private bool Reaches(float secondsLeft, float mark) =>
            _secondsLeft > mark && secondsLeft <= mark;

        private void Play(Sound sound) =>
            _player.Play(_lifetime, sound, _weapon);
    }
}
