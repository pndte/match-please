using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.View.Crosshair.Abstractions;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.View.Crosshair
{
    public sealed class WeaponRoundsCursor : IStateView<GunState>
    {
        private readonly ViewableProperty<int> _rounds;

        public WeaponRoundsCursor(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IReadonlyWeaponDraw draw,
            IAimCursor cursor,
            ShootingWeaponConfig config)
        {
            _rounds = new ViewableProperty<int>(config.AmmoSettings.OnSpawnValue);

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                draw.DrawnLifetime.WhenAlive(controlledLifetime, drawnLifetime =>
                    cursor.ShowRounds(drawnLifetime, _rounds, config.AmmoSettings.Max)));
        }

        public void Show(GunState from, GunState to, float progress) =>
            _rounds.Value = to.Ammo;
    }
}
