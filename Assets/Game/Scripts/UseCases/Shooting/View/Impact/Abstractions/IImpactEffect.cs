using Bw.Entities.Pool;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Impact.Abstractions
{
    public interface IImpactEffect : IOneShot
    {
        public void Play(Vector2 point, Vector2 direction, float delay);
    }
}
