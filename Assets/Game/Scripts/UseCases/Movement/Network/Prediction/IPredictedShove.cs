using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public interface IPredictedShove
    {
        public bool TryGetShown(out Vector2 position);
    }
}
