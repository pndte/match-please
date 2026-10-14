using Bw.UseCases.Audio.View.Playback.Abstractions;
using Bw.UseCases.Mortality.View.Hit;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.Audio
{
    public sealed class BodyHitSound
    {
        public BodyHitSound(
            Lifetime lifetime,
            IDamageViewModel damage,
            Transform body,
            ISoundPlayer player,
            CharacterSoundsConfig config)
        {
            damage.Taken.Advise(lifetime, _ => player.Play(config.BodyHit, body.position));
        }
    }
}
