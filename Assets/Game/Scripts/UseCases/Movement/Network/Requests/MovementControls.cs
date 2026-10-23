namespace Bw.UseCases.Movement.Network.Requests
{
    public readonly struct MovementControls
    {
        public readonly float Horizontal;
        public readonly bool JumpPressed;
        public readonly bool JumpHeld;
        public readonly bool Down;

        public MovementControls(
            float horizontal,
            bool jumpPressed,
            bool jumpHeld,
            bool down)
        {
            Horizontal = horizontal;
            JumpPressed = jumpPressed;
            JumpHeld = jumpHeld;
            Down = down;
        }
    }
}
