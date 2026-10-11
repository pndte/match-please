using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Vfx.View.Effects.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.View.Impact
{
    public sealed class StrikeImpactEffectsView
    {
        public StrikeImpactEffectsView(Lifetime lifetime, IStrikeImpacts impacts, IEffectPlayer effects, StrikeImpactConfig config)
        {
            impacts.Bodies.Advise(lifetime, impact => effects.Play(config.Body, impact.Point, impact.Direction));
            impacts.Surfaces.Advise(lifetime, impact => effects.Play(config.Surface, impact.Point, impact.Direction));
        }
    }
}
