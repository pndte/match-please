using Bw.Entities.Network.Prediction.Stream;
using Bw.UseCases.Movement.Extensions;

namespace Bw.UseCases.Movement.Network
{
    public sealed class MovementInputPolicy : IInputPolicy<MovementInput>
    {
        public bool IsValid(MovementInput input) =>
            float.IsFinite(input.Horizontal);

        public MovementInput Substitute(MovementInput lastInput) =>
            lastInput.WithoutJump();

        public MovementInput CaughtUp(
            MovementInput input,
            MovementInput lastInput,
            MovementInput heard,
            int heardTicksAgo) => //TODO: прыжок, нажатый в тике, чей ввод не дошёл до сервера вовремя (опоздал или пропал вместе с копией в следующем пакете), пропадает: клиент уже прыгнул, сервер нет, и сверка тянет персонажа обратно вниз; нужен такой же счётчик нажатий, как у курка (WeaponInputPolicy.CaughtUp)
            input;
    }
}
