using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.View.Crosshair.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Crosshair
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
