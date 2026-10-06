using Bw.Entities.Pool;
using UnityEngine;

namespace Bw.UseCases.Vfx.View.Effects.Abstractions
{
    public interface IEffect : IOneShot
    {
        public void Play(Vector2 point, Vector2 direction);
    }
}
