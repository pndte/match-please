using Bw.UseCases.Movement;
using UnityEngine;

namespace Bw.UseCases.Props.Extensions
{
    public static class PropThrowConfigExtensions
    {
        public static RigidState Thrown(
            this PropThrowConfig config,
            RigidState released,
            float aim,
            Vector2 holderVelocity)
        {
            var radians = aim * Mathf.Deg2Rad;
            var throwing = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * config.Speed;
            var velocity = throwing + new Vector2(holderVelocity.x * config.RunCarry, 0f);
            return new RigidState(released.Position, released.Angle, velocity, -Mathf.Sign(velocity.x) * config.Spin);
        }
    }
}
