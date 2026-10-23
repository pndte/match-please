using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Props.View.Held
{
    public sealed class HeldPropSorting
    {
        public HeldPropSorting(
            Lifetime lifetime,
            IWeaponCarryViewModel carry,
            SpriteRenderer sprite,
            int heldOrder)
        {
            carry.Held.WhenTrue(lifetime, heldLifetime =>
            {
                var freeOrder = sprite.sortingOrder;
                sprite.sortingOrder = heldOrder;
                heldLifetime.OnTermination(() => sprite.sortingOrder = freeOrder);
            });
        }
    }
}
