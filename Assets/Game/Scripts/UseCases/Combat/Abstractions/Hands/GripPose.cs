using UnityEngine;

namespace Bw.UseCases.Combat.Abstractions.Hands
{
    public readonly struct GripPose
    {
        public readonly Vector2 Center;
        public readonly float Angle;
        public readonly Vector2 Pushed;

        public GripPose(Vector2 center, float angle, Vector2 pushed)
        {
            Center = center;
            Angle = angle;
            Pushed = pushed;
        }
    }
}
