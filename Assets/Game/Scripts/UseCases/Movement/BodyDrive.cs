using UnityEngine;

namespace Bw.UseCases.Movement
{
    public readonly struct BodyDrive
    {
        public readonly Vector2 Velocity;
        public readonly float GravityScale;
        public readonly bool Dropping;

        public BodyDrive(Vector2 velocity, float gravityScale, bool dropping)
        {
            Velocity = velocity;
            GravityScale = gravityScale;
            Dropping = dropping;
        }

        public static BodyDrive Free(BodyState state) =>
            new(new Vector2(0f, state.Velocity.y), 1f, false);
    }
}
