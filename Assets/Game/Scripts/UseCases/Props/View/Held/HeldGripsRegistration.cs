using Bw.Entities.Extensions;
using Bw.UseCases.Character.View.Grips;
using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Props.View.Held
{
    public sealed class HeldGripsRegistration
    {
        public HeldGripsRegistration(
            Lifetime lifetime,
            IWeaponCarryViewModel carry,
            HandGrip[] grips,
            SpriteRenderer sprite)
        {
            carry.Held.WhenTrue(lifetime, heldLifetime => carry.Holder.GripView.Grip(heldLifetime, grips, sprite));
        }
    }
}
