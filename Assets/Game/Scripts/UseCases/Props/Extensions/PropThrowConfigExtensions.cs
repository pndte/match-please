using UnityEngine;

namespace Bw.UseCases.Props.Extensions
{
    public static class PropThrowConfigExtensions
    {
        public static Vector2 ThrowVelocity(this PropThrowConfig config, float aim, Vector2 holderVelocity)
        {
            var radians = aim * Mathf.Deg2Rad;
            var throwing = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * config.Speed;
            return throwing + new Vector2(holderVelocity.x * config.RunCarry, 0f);
        }

        public static float SpinOf(this PropThrowConfig config, Vector2 velocity) =>
            -Mathf.Sign(velocity.x) * config.Spin * Mathf.Clamp01(velocity.magnitude / config.Speed);
    }
}
