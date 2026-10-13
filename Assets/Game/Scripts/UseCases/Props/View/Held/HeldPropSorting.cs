using Bw.Entities.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Props.View.Held
{
    public sealed class HeldPropSorting
    {
        public HeldPropSorting(Lifetime lifetime, IWeaponHold hold, SpriteRenderer sprite, int heldOrder)
        {
            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
            {
                var freeOrder = sprite.sortingOrder;
                sprite.sortingOrder = heldOrder;
                heldLifetime.OnTermination(() => sprite.sortingOrder = freeOrder);
            });
        }
    }
}
