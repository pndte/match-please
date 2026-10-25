using UnityEngine;

namespace Bw.Entities.Network.Prediction.Bubble
{
    public interface IClientBubbles
    {
        public bool Contains(IClient client, Vector2 position, bool inside);
    }
}
