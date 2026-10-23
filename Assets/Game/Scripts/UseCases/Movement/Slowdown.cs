namespace Bw.UseCases.Movement
{
    public readonly struct Slowdown
    {
        public static readonly Slowdown None = default;

        public readonly float Speed;
        public readonly float JumpHeight;

        public Slowdown(float speed, float jumpHeight)
        {
            Speed = speed;
            JumpHeight = jumpHeight;
        }
    }
}
