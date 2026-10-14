using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Shooting.View.Crosshair.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.View.Crosshair
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
