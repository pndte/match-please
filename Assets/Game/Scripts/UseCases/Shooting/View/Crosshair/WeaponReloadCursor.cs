using Bw.Entities.Extensions;
using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Shooting.View.Crosshair.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.View.Crosshair
{
    public sealed class WeaponReloadCursor : IReloadTimer
    {
        public float Seconds => _gun.Reload.Value.Seconds;
        public float SecondsLeft => _gun.Reload.Value.SecondsLeft;

        private readonly IGunViewModel _gun;

        public WeaponReloadCursor(
            Lifetime lifetime,
            IWeaponCarryViewModel carry,
            IGunViewModel gun,
            IAimCursor cursor)
        {
            _gun = gun;

            carry.Mine.WhenTrue(lifetime, mineLifetime =>
                carry.Drawn.WhenTrue(mineLifetime, drawnLifetime =>
                    gun.Reload.Select(drawnLifetime, reload => reload.Active).WhenTrue(drawnLifetime, reloadLifetime =>
                        cursor.ShowReload(reloadLifetime, this))));
        }
    }
}
