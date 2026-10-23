using Bw.Entities.Extensions;
using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.View.Crosshair.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Crosshair
{
    public sealed class WeaponReloadCursor : IReloadTimer
    {
        public float Seconds => _gun.Reload.Value.Seconds; //TODO: когда перезарядка кончается, курсор ещё анимирует шкалу, а здесь уже пустая перезарядка (GunReload.None, 0 из 0 с): AimCursor делит 0 на 0, и каждый кадр в консоль летит «localRotation … NaN»; по той же причине щелчок «перезарядка закончена» звучит и после прерванной — нужно помнить последнюю активную перезарядку и чем она кончилась
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
