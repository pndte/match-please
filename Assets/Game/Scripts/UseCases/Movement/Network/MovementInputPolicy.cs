using Bw.Entities.Network.Prediction;
using Bw.UseCases.Movement.Extensions;

namespace Bw.UseCases.Movement.Network
{
    public sealed class MovementInputPolicy : IInputPolicy<MovementInput>
    {
        public bool IsValid(MovementInput input) =>
            float.IsFinite(input.Horizontal);

        public MovementInput Substitute(MovementInput lastInput) =>
            lastInput.WithoutJump();
    }
}
