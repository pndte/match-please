using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Audio.View.Playback.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Crates.View.Audio
{
    public sealed class CrateHitSound
    {
        public CrateHitSound(
            Lifetime lifetime,
            IReadonlyHealth health,
            Transform body,
            ISoundPlayer player,
            CrateSoundsConfig config)
        {
            health.AdviseDamage(lifetime, _ => player.Play(config.Hit, body.position));
        }
    }
}
