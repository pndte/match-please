namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class WeaponInputExtensions
    {
        public static WeaponInput WithoutReloadPress(this WeaponInput input) =>
            new(input.Aim, input.Trigger, false, input.ViewDelay);
    }
}
