namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class GunStateExtensions
    {
        public static GunState WithoutReload(this GunState state) =>
            new(state.Aim, state.Ammo, state.CooldownTicks, 0, state.Shots);
    }
}
