using UnityEngine;

namespace Bw.UseCases.Props.Extensions
{
    public static class HeldPropConfigExtensions
    {
        public static Vector2 HeldOffset(
            this HeldPropConfig config,
            float aim,
            Vector2 ovalRadii,
            Vector2 propHalfSize)
        {
            var radians = aim * Mathf.Deg2Rad;
            var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            return direction * (Vector2.Scale(ovalRadii, direction).magnitude + propHalfSize.x + config.Gap);
        }
    }
}
