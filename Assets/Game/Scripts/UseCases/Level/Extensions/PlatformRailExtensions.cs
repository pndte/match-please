using UnityEngine;

namespace Bw.UseCases.Level.Extensions
{
    public static class PlatformRailExtensions
    {
        public static Vector2 PositionAt(this PlatformRail rail, float along) =>
            rail.Start + rail.Travel * along;

        public static float AlongAt(this PlatformRail rail, Vector2 position) =>
            Vector2.Dot(position - rail.Start, rail.Travel) / rail.Travel.sqrMagnitude;

        public static Vector2 PositionAt(
            this PlatformRail rail,
            PlatformState from,
            PlatformState to,
            float progress) =>
            rail.PositionAt(Mathf.Lerp(from.Along, to.Along, progress));
    }
}
