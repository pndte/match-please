namespace Bw.UseCases.Movement
{
    public readonly struct MovementInput
    {
        public readonly float Horizontal;
        public readonly bool Jump;
        public readonly bool JumpHeld;

        public MovementInput(float horizontal, bool jump, bool jumpHeld)
        {
            Horizontal = horizontal;
            Jump = jump;
            JumpHeld = jumpHeld;
        }
    }
}
