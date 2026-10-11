using Bw.Entities.Pool;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Abstractions
{
    public interface IBulletTracer : IOneShot
    {
        public void Fly(Vector2 from, Vector2 to, float speed);
    }
}
