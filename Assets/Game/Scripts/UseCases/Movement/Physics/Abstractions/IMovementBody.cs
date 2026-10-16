using UnityEngine;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IMovementBody
    {
        public Vector2 Position { get; }
        public float Angle { get; }
        public void Place(Vector2 position);
    }
}
