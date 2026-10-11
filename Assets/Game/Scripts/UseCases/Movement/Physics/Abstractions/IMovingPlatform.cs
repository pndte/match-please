using UnityEngine;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IMovingPlatform
    {
        public Rect Area { get; }
        public Vector2 Shift { get; }
        public Rect AreaAt(double seconds);
    }
}
