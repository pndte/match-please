using Bw.UseCases.Mortality.View.Hit;
using DG.Tweening;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Crates.View.Damage
{
    public sealed class CrateHitJolt
    {
        public CrateHitJolt(
            Lifetime lifetime,
            IDamageViewModel damage,
            Transform visual,
            CrateDamageConfig config)
        {
            damage.Taken.Advise(lifetime, _ =>
            {
                visual.DOComplete();
                visual.DOPunchScale(config.HitPunch, config.HitPunchTime, 1, 0f).SetLink(visual.gameObject);
            });
        }
    }
}
