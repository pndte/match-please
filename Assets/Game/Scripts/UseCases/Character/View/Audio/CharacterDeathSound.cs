using Bw.Entities.Extensions;
using Bw.UseCases.Audio.View.Playback.Abstractions;
using Bw.UseCases.Character.View.Death;
using Bw.UseCases.Mortality.View.Death;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.Audio
{
    public sealed class CharacterDeathSound
    {
        private readonly Transform _body;
        private readonly ISoundPlayer _player;
        private readonly CharacterSoundsConfig _sounds;
        private readonly CharacterDeathViewConfig _deathView;

        public CharacterDeathSound(
            Lifetime lifetime,
            IDeathViewModel death,
            Transform body,
            ISoundPlayer player,
            CharacterSoundsConfig sounds,
            CharacterDeathViewConfig deathView)
        {
            _body = body;
            _player = player;
            _sounds = sounds;
            _deathView = deathView;

            death.Killed.Advise(lifetime, _ => lifetime.WhenElapsed(deathView.BurstTime, Burst));
        }

        private void Burst() =>
            _player.Play(_sounds.Death, (Vector2)_body.position + _deathView.BurstOffset);
    }
}
