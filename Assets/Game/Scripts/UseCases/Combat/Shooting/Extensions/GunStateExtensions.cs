using System;

namespace Bw.UseCases.Combat.Shooting.Extensions
{
    public static class GunStateExtensions
    {
        public static GunState WithoutReload(this GunState state) =>
            new(state.Aim, state.Ammo, state.CooldownTicks, 0, state.Shots);

        public static GunState CooledDown(this GunState state) =>
            new(state.Aim, state.Ammo, Math.Max(0, state.CooldownTicks - 1), state.ReloadTicks, state.Shots);
    }
}
