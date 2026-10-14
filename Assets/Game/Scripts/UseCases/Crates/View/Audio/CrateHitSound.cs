using Bw.UseCases.Audio.View.Playback.Abstractions;
using Bw.UseCases.Mortality.View.Hit;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Crates.View.Audio
{
    public sealed class CrateHitSound
    {
        public CrateHitSound(
            Lifetime lifetime,
            IDamageViewModel damage,
            Transform body,
            ISoundPlayer player,
            CrateSoundsConfig config)
        {
            damage.Taken.Advise(lifetime, _ => player.Play(config.Hit, body.position));
        }
    }
}
