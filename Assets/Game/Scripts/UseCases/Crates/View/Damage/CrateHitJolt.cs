using Bw.Entities;
using Bw.Entities.Extensions;
using DG.Tweening;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Crates.View.Damage
{
    public sealed class CrateHitJolt
    {
        public CrateHitJolt(
            Lifetime lifetime,
            IReadonlyHealth health,
            Transform visual,
            CrateDamageConfig config)
        {
            health.AdviseDamage(lifetime, _ =>
            {
                visual.DOComplete();
                visual.DOPunchScale(config.HitPunch, config.HitPunchTime, 1, 0f).SetLink(visual.gameObject);
            });
        }
    }
}
