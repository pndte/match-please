namespace Bw.UseCases.Movement
{
    public readonly struct MovementInput
    {
        public readonly float Horizontal;
        public readonly bool Jump;
        public readonly bool JumpHeld;
        public readonly bool Down;

        public MovementInput(
            float horizontal,
            bool jump,
            bool jumpHeld,
            bool down)
        {
            Horizontal = horizontal;
            Jump = jump;
            JumpHeld = jumpHeld;
            Down = down;
        }
    }
}
