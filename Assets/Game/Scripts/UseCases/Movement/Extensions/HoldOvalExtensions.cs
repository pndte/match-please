using UnityEngine;

namespace Bw.UseCases.Movement.Extensions
{
    public static class HoldOvalExtensions
    {
        public static Rect WorldRect(this HoldOval oval)
        {
            var transform = oval.transform;
            var scale = (Vector2)transform.lossyScale;
            var radii = new Vector2(oval.Radii.x * Mathf.Abs(scale.x), oval.Radii.y * Mathf.Abs(scale.y));
            var center = (Vector2)transform.position + Vector2.Scale(oval.Center, scale);
            return new Rect(center - radii, radii * 2f);
        }
    }
}
