using Bw.Entities.Network.Prediction.Bubble;
using Bw.UseCases.Combat.Abstractions.Characters;
using UnityEngine;

namespace Bw.UseCases.Character.Network
{
    public sealed class PredictionBubble : IPredictionBubble
    {
        private readonly IControlledCharacters _centers;
        private readonly PredictionBubbleConfig _config;

        public PredictionBubble(IControlledCharacters centers, PredictionBubbleConfig config)
        {
            _centers = centers;
            _config = config;
        }

        public bool Contains(Vector2 position, bool inside)
        {
            var radius = inside ? _config.LeaveRadius : _config.EnterRadius;
            for (var index = 0; index < _centers.Count; index++)
                if ((_centers[index].Movement.State.Value.Body.Position - position).sqrMagnitude <= radius * radius)
                    return true;

            return false;
        }
    }
}
