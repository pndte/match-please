using System;
using UnityEngine;

namespace Bw.Entities.Network.Prediction.Bubble
{
    [Serializable]
    public sealed class PredictionBubbleConfig
    {
        [Min(0f)] public float EnterRadius = 14f;
        [Min(0f)] public float LeaveRadius = 18f;
    }
}
