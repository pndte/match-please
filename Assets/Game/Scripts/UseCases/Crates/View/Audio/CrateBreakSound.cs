using Bw.UseCases.Audio.View.Playback.Abstractions;
using Bw.UseCases.Mortality.View.Death;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Crates.View.Audio
{
    public sealed class CrateBreakSound
    {
        public CrateBreakSound(
            Lifetime lifetime,
            IDeathViewModel death,
            Transform body,
            ISoundPlayer player,
            CrateSoundsConfig config)
        {
            death.Killed.Advise(lifetime, _ => player.Play(config.Break, body.position));
        }
    }
}
