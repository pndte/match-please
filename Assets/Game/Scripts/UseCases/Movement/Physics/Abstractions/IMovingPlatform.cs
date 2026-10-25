using UnityEngine;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IMovingPlatform
    {
        public int BoxCount { get; }
        public Vector2 Shift { get; }
        public MovingBox BoxAt(int index);
    }
}
