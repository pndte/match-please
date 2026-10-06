using Bw.Entities.Network.Prediction;
using Bw.UseCases.Shooting.Weapon.Extensions;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class WeaponInputPolicy : IInputPolicy<WeaponInput>
    {
        public bool IsValid(WeaponInput input) =>
            float.IsFinite(input.Aim) && float.IsFinite(input.ViewDelay);

        public WeaponInput Substitute(WeaponInput lastInput) =>
            lastInput.WithoutPresses();
    }
}
