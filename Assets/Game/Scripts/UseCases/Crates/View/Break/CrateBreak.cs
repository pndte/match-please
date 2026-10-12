using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Vfx.View.Effects.Abstractions;
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
            IReadonlyMortal crate,
            SpriteRenderer sprite,
            Transform body,
            IEffectPlayer effects,
            CrateBreakConfig config)
        {
            _body = body;
            _effects = effects;
            _config = config;

            crate.State.WhenDead(lifetime, _ => sprite.enabled = false);
            crate.AdviseKilled(lifetime, Burst);
        }

        private void Burst() =>
            _effects.Play(_config.Debris, (Vector2)_body.position + _config.DebrisOffset, Vector2.up);
    }
}
