namespace Bw.UseCases.Movement.Extensions
{
    public static class MovementInputExtensions
    {
        public static MovementInput WithoutJump(this MovementInput input) =>
            new(input.Horizontal, false, input.JumpHeld);
    }
}
