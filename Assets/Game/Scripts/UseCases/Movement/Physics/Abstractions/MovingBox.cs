using UnityEngine;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public readonly struct MovingBox
    {
        public readonly Rect Area;
        public readonly bool IsOneWay;

        public MovingBox(Rect area, bool isOneWay)
        {
            Area = area;
            IsOneWay = isOneWay;
        }
    }
}
