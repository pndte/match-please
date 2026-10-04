using UnityEngine;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IMovementBody
    {
        public Vector2 Position { get; }
        public void Place(Vector2 position);
    }
}
