using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Shooting.View.Crosshair.Abstractions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.View.Crosshair
{
    public sealed class FistsCursor
    {
        public FistsCursor(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IReadonlyWeaponDraw draw,
            IAimCursor cursor)
        {
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                draw.DrawnLifetime.WhenAlive(controlledLifetime, cursor.ShowFists));
        }
    }
}
