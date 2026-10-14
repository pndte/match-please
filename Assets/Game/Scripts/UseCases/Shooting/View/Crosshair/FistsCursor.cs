using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Shooting.View.Crosshair.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.View.Crosshair
{
    public sealed class FistsCursor
    {
        public FistsCursor(Lifetime lifetime, IWeaponCarryViewModel carry, IAimCursor cursor)
        {
            carry.Mine.WhenTrue(lifetime, mineLifetime =>
                carry.Drawn.WhenTrue(mineLifetime, cursor.ShowFists));
        }
    }
}
