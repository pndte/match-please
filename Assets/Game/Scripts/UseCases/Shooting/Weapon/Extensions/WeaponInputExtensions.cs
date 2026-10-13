using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class WeaponInputExtensions
    {
        public static WeaponInput WithoutReloadPress(this WeaponInput input) =>
            new(input.Aim, input.Trigger, false, input.ViewDelay, input.PredictedDelay);

        public static WeaponInput Deflected(this WeaponInput input, float degrees) =>
            new(Mathf.DeltaAngle(0f, input.Aim + degrees), input.Trigger, input.Reload, input.ViewDelay, input.PredictedDelay);
    }
}
