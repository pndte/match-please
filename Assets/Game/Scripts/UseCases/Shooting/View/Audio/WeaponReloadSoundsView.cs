using Bw.UseCases.Audio.View.Playback.Abstractions;
using Bw.UseCases.Shooting.View.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Audio
{
    public sealed class WeaponReloadSoundsView
    {
        private readonly Lifetime _lifetime;
        private readonly Transform _weapon;
        private readonly ISoundPlayer _player;
        private readonly GunSounds _sounds;

        private bool _reloading;
        private float _progress;

        public WeaponReloadSoundsView(
            Lifetime lifetime,
            IGunViewModel gun,
            Transform weapon,
            ISoundPlayer player,
            GunSounds sounds)
        {
            _lifetime = lifetime;
            _weapon = weapon;
            _player = player;
            _sounds = sounds;

            gun.Reload.Advise(lifetime, Follow);
        }

        private void Follow(GunReload reload)
        {
            if (!reload.Active)
            {
                _reloading = false;
                return;
            }

            if (!_reloading)
            {
                _reloading = true;
                _progress = reload.Progress;
            }

            foreach (var cue in _sounds.Reload)
                if ((!cue.OnlyEmpty || reload.Empty) && _progress < cue.At && reload.Progress >= cue.At)
                    _player.Play(_lifetime, cue.Sound, _weapon);

            _progress = reload.Progress;
        }
    }
}
