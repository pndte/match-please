using UnityEngine;

namespace Bw.Entities.Network.Prediction.Bubble
{
    public static class PredictionBubbleConfigExtensions
    {
        public static bool InputsNeeded(this PredictionBubbleConfig config, Vector2 center, Vector2 position)
        {
            var radius = config.LeaveRadius + config.InputRelayMargin;
            return (position - center).sqrMagnitude <= radius * radius;
        }
    }
}
