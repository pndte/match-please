using Bw.Entities.Extensions;
using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Character.View.Grips;
using Bw.UseCases.Shooting.View.Abstractions;
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
            Transform prop,
            HandGrip[] grips,
            SpriteRenderer sprite)
        {
            carry.Held.WhenTrue(lifetime, heldLifetime => prop.HolderService<IGripView>().Grip(heldLifetime, grips, sprite));
        }
    }
}
