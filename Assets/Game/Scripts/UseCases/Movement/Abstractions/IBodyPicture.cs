using UnityEngine;

namespace Bw.UseCases.Movement.Abstractions
{
    public interface IBodyPicture
    {
        public Vector2 Position { get; }
        public void Place(Vector2 position);
    }
}
