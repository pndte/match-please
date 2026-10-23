namespace Bw.UseCases.Movement
{
    public readonly struct MovementInput
    {
        public readonly float Horizontal;
        public readonly bool JumpPressed;
        public readonly bool JumpHeld;
        public readonly bool Down;
        public readonly Slowdown Slowdown;

        public MovementInput(
            float horizontal,
            bool jumpPressed,
            bool jumpHeld,
            bool down,
            Slowdown slowdown)
        {
            Horizontal = horizontal;
            JumpPressed = jumpPressed;
            JumpHeld = jumpHeld;
            Down = down;
            Slowdown = slowdown;
        }
    }
}
