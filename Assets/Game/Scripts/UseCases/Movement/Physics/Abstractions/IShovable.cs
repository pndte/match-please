using UnityEngine;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IShovable
    {
        public float RetreatSpeed(Vector2 direction);
        public void Shove(Vector2 force);
    }
}
