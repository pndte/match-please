using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public interface IPushRules<TEffect> where TEffect : struct
    {
        public Vector2 Impulse(TEffect effect);
    }
}
