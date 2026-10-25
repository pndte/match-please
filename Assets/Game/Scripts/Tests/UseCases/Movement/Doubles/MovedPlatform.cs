using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement.Doubles
{
    public sealed class MovedPlatform : IMovingPlatform
    {
        public Rect Area { get; set; }
        public bool OneWay { get; set; }
        public Vector2 Shift { get; set; }
        public int BoxCount => 1;

        public MovedPlatform(Rect area, Vector2 shift)
        {
            Area = area;
            Shift = shift;
        }

        public MovingBox BoxAt(int index) =>
            new(Area, OneWay);
    }
}
