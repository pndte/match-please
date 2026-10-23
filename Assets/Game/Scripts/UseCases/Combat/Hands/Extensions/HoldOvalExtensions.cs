using Bw.UseCases.Combat.Abstractions.Hands;
using UnityEngine;

namespace Bw.UseCases.Combat.Hands.Extensions
{
    public static class HoldOvalExtensions
    {
        public static HoldPose PoseAt(this HoldOval oval, Vector2 position) =>
            new(position, new Rect(position + oval.Center - oval.Radii, oval.Radii * 2f));
    }
}
