using UnityEngine;

namespace Bw.UseCases.Combat.Abstractions.Hands
{
    public readonly struct HoldPose
    {
        public readonly Vector2 Position;
        public readonly Rect Hold;

        public HoldPose(Vector2 position, Rect hold)
        {
            Position = position;
            Hold = hold;
        }
    }
}
