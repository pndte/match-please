using UnityEngine;

namespace Bw.UseCases.Camera.View.Shake
{
    public interface ICameraShake
    {
        public void Shake(float strength);
        public void Kick(Vector2 offset);
    }
}
