using Bw.Entities.Extensions;
using Bw.UseCases.Audio.View.Playback.Abstractions;
using Bw.UseCases.Shooting.View.Audio.Abstractions;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Audio
{
    public sealed class ShotSfxPlayer : IShotSfxPlayer
    {
        private readonly Lifetime _lifetime;
        private readonly IWeaponMuzzle _muzzle;
        private readonly ISoundPlayer _player;
        private readonly WeaponSoundsConfig _config;

        public ShotSfxPlayer(
            Lifetime lifetime,
            IWeaponMuzzle muzzle,
            ISoundPlayer player,
            WeaponSoundsConfig config)
        {
            _lifetime = lifetime;
            _muzzle = muzzle;
            _player = player;
            _config = config;
        }

        public void Play(ShotTrace trace, float arrival)
        {
            _player.Play(_lifetime, _config.Shot, _muzzle.Transform);
            if (trace.Hit && HitsGround(trace.Hit.collider))
                _lifetime.WhenElapsed(arrival, () => _player.Play(_config.GroundHit, trace.To));
        }

        private bool HitsGround(Collider2D collider) =>
            (_config.GroundLayers.value & (1 << collider.gameObject.layer)) != 0; //TODO: вся земля звучит как трава — для других поверхностей нужен звук попадания у самой поверхности
    }
}
