using Bw.UseCases.Audio.View.Playback.Abstractions;
using Bw.UseCases.Shooting.View.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Audio
{
    public sealed class MeleeSoundsView
    {
        public MeleeSoundsView(
            Lifetime lifetime,
            Transform weapon,
            IMeleeSwingsViewModel swings,
            IStrikeImpacts impacts,
            ISoundPlayer player,
            MeleeSounds sounds)
        {
            swings.Started.Advise(lifetime, _ => player.Play(lifetime, sounds.Swing, weapon));
            impacts.Bodies.Advise(lifetime, impact => player.Play(sounds.BodyHit, impact.Point));
            impacts.Surfaces.Advise(lifetime, impact => player.Play(sounds.SurfaceHit, impact.Point));
        }
    }
}
