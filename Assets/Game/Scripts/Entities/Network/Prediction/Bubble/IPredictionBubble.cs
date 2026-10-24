using UnityEngine;

namespace Bw.Entities.Network.Prediction.Bubble
{
    public interface IPredictionBubble
    {
        public bool Contains(Vector2 position, bool inside);
    }
}
