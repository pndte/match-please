using Bw.UseCases.Combat.Weapon.Pickup.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Adapters
{
    public sealed class PickupTargetHighlight
    {
        public PickupTargetHighlight(Lifetime lifetime, IPickupTargeting targeting)
        {
            targeting.Target.View(lifetime, (targetLifetime, target) =>
                target.Switch(() => { }, weapon => weapon.Highlight(targetLifetime)));
        }
    }
}
