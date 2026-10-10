using UnityEngine;

namespace Bw.UseCases.Shooting.View.Crosshair.Abstractions
{
    public interface ISystemCursor
    {
        public Vector2 Position { get; }
        public void MoveTo(Vector2 position);
    }
}
