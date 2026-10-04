namespace Bw.UseCases.Movement
{
    public readonly struct MovementInput
    {
        public readonly float Horizontal;
        public readonly bool Jump;

        public MovementInput(float horizontal, bool jump)
        {
            Horizontal = horizontal;
            Jump = jump;
        }
    }
}
