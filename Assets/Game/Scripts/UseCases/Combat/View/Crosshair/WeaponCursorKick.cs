using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.View.Crosshair.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Crosshair
{
    public sealed class WeaponCursorKick
    {
        public WeaponCursorKick(
            Lifetime lifetime,
            IWeaponCarryViewModel carry,
            IShotsViewModel shots,
            IAimCursor cursor)
        {
            carry.Mine.WhenTrue(lifetime, mineLifetime =>
                shots.Seen.Advise(mineLifetime, _ => cursor.ShowShot()));
        }
    }
}
