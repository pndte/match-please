using Bw.UseCases.Mortality.View.Death;
using Bw.UseCases.Vfx.View.Effects.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Crates.View.Break
{
    public sealed class CrateBreak
    {
        private readonly Transform _body;
        private readonly IEffectPlayer _effects;
        private readonly CrateBreakConfig _config;

        public CrateBreak(
            Lifetime lifetime,
            IDeathViewModel death,
            SpriteRenderer sprite,
            Transform body,
            IEffectPlayer effects,
            CrateBreakConfig config)
        {
            _body = body;
            _effects = effects;
            _config = config;

            death.Dead.WhenTrue(lifetime, _ => sprite.enabled = false);
            death.Killed.Advise(lifetime, _ => Burst());
        }

        private void Burst() =>
            _effects.Play(_config.Debris, (Vector2)_body.position + _config.DebrisOffset, Vector2.up);
    }
}
