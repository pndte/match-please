namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class WeaponInputExtensions
    {
        public static WeaponInput WithoutPresses(this WeaponInput input) =>
            new(input.Aim, false, false);
    }
}
