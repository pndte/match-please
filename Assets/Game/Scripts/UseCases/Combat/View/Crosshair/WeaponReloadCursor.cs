using Bw.Entities.Extensions;
using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.View.Crosshair.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Crosshair
{
    public sealed class WeaponReloadCursor : IReloadTimer
    {
        public float Seconds => _shown.Seconds;
        public float SecondsLeft => _refilled ? 0f : _shown.SecondsLeft;

        private readonly IGunViewModel _gun;
        private readonly IAimCursor _cursor;
        private GunReload _shown;
        private bool _refilled;

        public WeaponReloadCursor(
            Lifetime lifetime,
            IWeaponCarryViewModel carry,
            IGunViewModel gun,
            IAimCursor cursor)
        {
            _gun = gun;
            _cursor = cursor;

            carry.Mine.WhenTrue(lifetime, mineLifetime =>
                carry.Drawn.WhenTrue(mineLifetime, drawnLifetime =>
                    gun.Reload.Select(drawnLifetime, reload => reload.Active).WhenTrue(drawnLifetime, Show)));
        }

        private void Show(Lifetime reloadLifetime)
        {
            var rounds = _gun.Rounds.Value;
            _refilled = false;
            _gun.Reload.Advise(reloadLifetime, Remember);
            _gun.Rounds.Advise(reloadLifetime, now => Refill(rounds, now));
            _cursor.ShowReload(reloadLifetime, this);
        }

        private void Remember(GunReload reload)
        {
            if (reload.Active)
                _shown = reload;
        }

        private void Refill(int before, int now)
        {
            if (now > before)
                _refilled = true;
        }
    }
}
