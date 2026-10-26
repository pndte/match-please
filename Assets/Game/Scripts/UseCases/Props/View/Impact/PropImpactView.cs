using Bw.UseCases.Props.View.Flight;
using Bw.UseCases.Vfx.View.Effects.Abstractions;
using DG.Tweening;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Props.View.Impact
{
    public sealed class PropImpactView
    {
        public PropImpactView(
            Lifetime lifetime,
            PropMotionTracker motionTracker,
            IEffectPlayer effects,
            Transform visual,
            PropVfxConfig config)
        {
            motionTracker.Impacted.Advise(lifetime, impact =>
            {
                effects.Play(config.Dust, impact.Point, impact.Normal);
                visual.DOComplete();
                visual.DOPunchScale(config.LandPunch, config.LandPunchTime, 1, 0f).SetLink(visual.gameObject);
            });
        }
    }
}
