using Bw.Entities.Extensions;
using Bw.UseCases.Audio.View.Playback.Abstractions;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Character.View.Death;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.Audio
{
    public sealed class CharacterDeathSound
    {
        private readonly Transform _body;
        private readonly ISoundPlayer _player;
        private readonly CharacterSoundsConfig _sounds;
        private readonly CharacterDeathViewConfig _death;

        public CharacterDeathSound(
            Lifetime lifetime,
            IReadonlyMortal character,
            Transform body,
            ISoundPlayer player,
            CharacterSoundsConfig sounds,
            CharacterDeathViewConfig death)
        {
            _body = body;
            _player = player;
            _sounds = sounds;
            _death = death;

            character.AdviseKilled(lifetime, () => lifetime.WhenElapsed(death.BurstTime, Burst));
        }

        private void Burst() =>
            _player.Play(_sounds.Death, (Vector2)_body.position + _death.BurstOffset);
    }
}
