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
            int heardTicksAgo) =>
            controls;
    }
}
