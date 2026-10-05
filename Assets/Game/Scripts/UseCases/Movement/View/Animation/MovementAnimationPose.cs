namespace Bw.UseCases.Movement.View.Animation
{
    public readonly struct MovementAnimationPose
    {
        public readonly float Speed;
        public readonly float VerticalSpeed;
        public readonly bool Grounded;
        public readonly float Facing;

        public MovementAnimationPose(float speed, float verticalSpeed, bool grounded, float facing)
        {
            Speed = speed;
            VerticalSpeed = verticalSpeed;
            Grounded = grounded;
            Facing = facing;
        }
    }
}
