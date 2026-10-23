using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props
{
    public sealed class HeldBoxRegistration
    {
        public HeldBoxRegistration(
            Lifetime lifetime,
            IWeapon prop,
            IHeldBox box,
            IHeldBoxes boxes)
        {
            boxes.ByWeapon.AddLifetimed(lifetime, prop, box);
        }
    }
}
