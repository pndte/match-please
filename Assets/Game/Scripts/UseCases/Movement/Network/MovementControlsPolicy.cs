using Bw.Entities.Network.Prediction.Stream;
using Bw.UseCases.Movement.Network.Requests;

namespace Bw.UseCases.Movement.Network
{
    public sealed class MovementControlsPolicy : IInputPolicy<MovementControls>
    {
        public bool IsValid(MovementControls controls) =>
            float.IsFinite(controls.Horizontal);

        public MovementControls CaughtUp(
            MovementControls controls,
            MovementControls lastControls,
            MovementControls heard,
            int heardTicksAgo) => //TODO: прыжок, нажатый в тике, чей ввод не дошёл до сервера вовремя (опоздал или пропал вместе с копией в следующем пакете), пропадает: клиент уже прыгнул, сервер нет, и сверка тянет персонажа обратно вниз; нужен такой же счётчик нажатий, как у курка (HandsControlsPolicy.CaughtUp)
            controls;
    }
}
