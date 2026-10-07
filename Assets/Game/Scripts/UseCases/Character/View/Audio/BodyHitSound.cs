using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Audio.View.Playback.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.Audio
{
    public sealed class BodyHitSound
    {
        public BodyHitSound(
            Lifetime lifetime,
            IReadonlyHealth health,
            Transform body,
            ISoundPlayer player,
            CharacterSoundsConfig config)
        {
            health.AdviseDamage(lifetime, _ => player.Play(config.BodyHit, body.position));
        }
    }
}
