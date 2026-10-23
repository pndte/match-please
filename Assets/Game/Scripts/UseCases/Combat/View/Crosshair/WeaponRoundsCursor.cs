using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.View.Crosshair.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Crosshair
{
    public sealed class WeaponRoundsCursor
    {
        public WeaponRoundsCursor(
            Lifetime lifetime,
            IWeaponCarryViewModel carry,
            IGunViewModel gun,
            IAimCursor cursor)
        {
            carry.Mine.WhenTrue(lifetime, mineLifetime =>
                carry.Drawn.WhenTrue(mineLifetime, drawnLifetime =>
                    cursor.ShowRounds(drawnLifetime, gun.Rounds, gun.Capacity)));
        }
    }
}
