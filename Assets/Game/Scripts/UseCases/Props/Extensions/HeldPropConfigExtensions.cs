using UnityEngine;

namespace Bw.UseCases.Props.Extensions
{
    public static class HeldPropConfigExtensions
    {
        public static Vector2 HeldOffset(
            this HeldPropConfig config,
            float aim,
            Vector2 holderHalfSize,
            Vector2 propHalfSize)
        {
            var reach = holderHalfSize + propHalfSize + new Vector2(config.Gap, config.Gap);
            var radians = aim * Mathf.Deg2Rad;
            var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            return direction * Mathf.Min(Along(reach.x, direction.x), Along(reach.y, direction.y));
        }

        private static float Along(float reach, float component) =>
            component == 0f ? float.PositiveInfinity : reach / Mathf.Abs(component);
    }
}
