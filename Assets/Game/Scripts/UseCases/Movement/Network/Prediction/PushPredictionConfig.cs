using System;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    [Serializable]
    public sealed class PushPredictionConfig
    {
        [Min(0.01f)] public float RejectedFadeSeconds = 0.15f;
    }
}
