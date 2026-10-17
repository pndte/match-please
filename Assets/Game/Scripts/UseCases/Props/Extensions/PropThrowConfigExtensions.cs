using Bw.UseCases.Movement;
using UnityEngine;

namespace Bw.UseCases.Props.Extensions
{
    public static class PropThrowConfigExtensions
    {
        public static RigidState Thrown(this PropThrowConfig config, RigidState released, float aim)
        {
            var radians = aim * Mathf.Deg2Rad;
            var velocity = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * config.Speed;
            return new RigidState(released.Position, released.Angle, velocity, -Mathf.Sign(velocity.x) * config.Spin);
        }
    }
}
