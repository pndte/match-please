using System;
using UnityEngine;

namespace Bw.Entities.Network.Prediction.Events
{
    [Serializable]
    public sealed class EventPredictionConfig
    {
        [Min(1)] public int ExpiryTicks = 15;
    }
}
