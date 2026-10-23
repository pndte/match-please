using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Weapon.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class DrawnWeaponControl
    {
        public DrawnWeaponControl(
            Lifetime lifetime,
            HandsDraw draw,
            IControlledBy controlledBy,
            IWeaponControlledBy weapons)
        {
            draw.Drawn.View(lifetime, (drawnLifetime, weapon) =>
                controlledBy.Me.WhenTrue(drawnLifetime, weapons.ByWeapon[weapon].SetMe));
        }
    }
}
