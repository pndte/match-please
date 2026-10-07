using UnityEngine;

namespace Bw.UseCases.Vfx.View.Effects.Abstractions
{
    public interface IEffectPlayer
    {
        public void Play(GameObject prefab, Vector2 point, Vector2 direction);
    }
}
