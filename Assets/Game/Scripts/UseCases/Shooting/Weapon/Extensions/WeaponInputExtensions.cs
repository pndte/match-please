using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class WeaponInputExtensions
    {
        public static WeaponInput WithoutPresses(this WeaponInput input) =>
            new(input.Aim, input.Trigger && input.Press.Age > 0, false, input.ViewDelay, input.PredictedDelay, input.Press);

        public static WeaponInput Deflected(this WeaponInput input, float degrees) =>
            new(Mathf.DeltaAngle(0f, input.Aim + degrees), input.Trigger, input.Reload, input.ViewDelay, input.PredictedDelay, input.Press);

        public static WeaponInput Pressed(this WeaponInput input, TriggerPress press, int lateTicks) =>
            new(input.Aim, true, input.Reload, press.ViewDelay + lateTicks, press.PredictedDelay + lateTicks, press.Delivered(lateTicks));

        public static WeaponInput WithPress(this WeaponInput input, TriggerPress press) =>
            new(input.Aim, input.Trigger, input.Reload, input.ViewDelay, input.PredictedDelay, press);
    }
}
