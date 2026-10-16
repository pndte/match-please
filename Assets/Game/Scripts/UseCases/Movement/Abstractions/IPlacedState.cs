using UnityEngine;

namespace Bw.UseCases.Movement.Abstractions
{
    public interface IPlacedState<TState> where TState : struct
    {
        public Vector2 Position { get; }
        public float Angle { get; }
        public TState Shifted(Vector2 offset);
    }
}
