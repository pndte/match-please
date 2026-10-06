using Bw.Entities.Network.Prediction;
using Bw.UseCases.Shooting.Weapon.Extensions;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class WeaponInputPolicy : IInputPolicy<WeaponInput>
    {
        public bool IsValid(WeaponInput input) =>
            float.IsFinite(input.Aim) && float.IsFinite(input.ViewDelay);

        public WeaponInput Substitute(WeaponInput lastInput) => //TODO: зажатый курок повторяется — если игрок отпустил кнопку ровно в потерянных тиках, сервер выстрелит лишний раз
            lastInput.WithoutReloadPress();
    }
}
