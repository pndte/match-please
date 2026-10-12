using Bw.Entities;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Crates.View.Damage
{
    public sealed class CrateCracks
    {
        public CrateCracks(Lifetime lifetime, IReadonlyHealth health, SpriteRenderer sprite, CrateDamageConfig config)
        {
            var intact = sprite.sprite;
            health.State.Advise(lifetime, state =>
                sprite.sprite = state.Current <= health.Max * config.CrackedBelow ? config.Cracked : intact);
        }
    }
}
