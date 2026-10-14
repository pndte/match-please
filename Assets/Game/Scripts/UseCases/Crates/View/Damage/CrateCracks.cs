using Bw.UseCases.Mortality.View.Health;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Crates.View.Damage
{
    public sealed class CrateCracks
    {
        public CrateCracks(
            Lifetime lifetime,
            IHealthViewModel health,
            SpriteRenderer sprite,
            CrateDamageConfig config)
        {
            var intact = sprite.sprite;
            health.Level.Advise(lifetime, level =>
                sprite.sprite = level.Share <= config.CrackedBelow ? config.Cracked : intact);
        }
    }
}
