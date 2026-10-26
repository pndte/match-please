using Bw.Entities.Extensions;
using Bw.UseCases.Audio.View.Playback.Abstractions;
using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.View.Audio.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Combat.View.Audio
{
    public sealed class ShotSfxPlayer : IShotSfxPlayer
    {
        private readonly Lifetime _lifetime;
        private readonly Transform _muzzle;
        private readonly ISoundPlayer _player;
        private readonly GunSounds _sounds;
        private readonly ShotImpactSoundsConfig _impacts;

        public ShotSfxPlayer(
            Lifetime lifetime,
            Transform muzzle,
            ISoundPlayer player,
            GunSounds sounds,
            ShotImpactSoundsConfig impacts)
        {
            _lifetime = lifetime;
            _muzzle = muzzle;
            _player = player;
            _sounds = sounds;
            _impacts = impacts;
        }

        public void Play() =>
            _player.Play(_lifetime, _sounds.Shot, _muzzle);

        public void PlayImpact(PelletTrace trace, float arrival)
        {
            if (trace.End == PelletEnd.Struck && IsGround(trace.Layer))
                _lifetime.WhenElapsed(arrival, () => _player.Play(_impacts.GroundHit, trace.To));
        }

        private bool IsGround(int layer) =>
            (_impacts.GroundLayers.value & (1 << layer)) != 0; //TODO: придумать звуки, зависящие от других поверхностей
    }
}
