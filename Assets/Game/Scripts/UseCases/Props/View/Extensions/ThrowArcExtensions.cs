using UnityEngine;

namespace Bw.UseCases.Props.View.Extensions
{
    public static class ThrowArcExtensions
    {
        public static Vector2 PointAt(this ThrowArc arc, float time) =>
            arc.Origin + arc.Velocity * time + arc.Gravity * (0.5f * time * time);
    }
}
