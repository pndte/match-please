using Bw.UseCases.Audio.View.Playback.Abstractions;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Crates.View.Audio
{
    public sealed class CrateBreakSound
    {
        public CrateBreakSound(
            Lifetime lifetime,
            IReadonlyMortal crate,
            Transform body,
            ISoundPlayer player,
            CrateSoundsConfig config)
        {
            crate.AdviseKilled(lifetime, () => player.Play(config.Break, body.position));
        }
    }
}
