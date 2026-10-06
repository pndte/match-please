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
        private readonly LineRendererVfxConfig _trailConfig;

        public ShotSfxPlayer(
            Lifetime lifetime,
            IWeaponMuzzle muzzle,
            ISoundPlayer player,
            WeaponSoundsConfig config,
            LineRendererVfxConfig trailConfig)
        {
            _lifetime = lifetime;
            _muzzle = muzzle;
            _player = player;
            _config = config;
            _trailConfig = trailConfig;
        }

        public void Play(ShotTrace trace)
        {
            _player.Play(_lifetime, _config.Shot, _muzzle.Transform);
            //TODO: время подлёта пули считается так же, как задержка частиц попадания в WeaponShotEffectsView — вынести в одно место
            if (trace.Hit && HitsGround(trace.Hit.collider))
                _player.PlayDelayed(_config.GroundHit, trace.To, Vector2.Distance(trace.From, trace.To) / _trailConfig.TrailSpeed);
        }

        private bool HitsGround(Collider2D collider) =>
            (_config.GroundLayers.value & (1 << collider.gameObject.layer)) != 0; //TODO: вся земля звучит как трава — для других поверхностей нужен звук попадания у самой поверхности
    }
}
