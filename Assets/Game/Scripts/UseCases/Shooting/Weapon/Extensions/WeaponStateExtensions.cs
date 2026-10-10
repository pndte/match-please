namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class WeaponStateExtensions
    {
        public static WeaponState WithoutReload(this WeaponState state) =>
            new(state.Aim, state.Ammo, state.CooldownTicks, 0, state.Shots);

        public static WeaponState WithoutCooldown(this WeaponState state) =>
            new(state.Aim, state.Ammo, 0, state.ReloadTicks, state.Shots);
    }
}
