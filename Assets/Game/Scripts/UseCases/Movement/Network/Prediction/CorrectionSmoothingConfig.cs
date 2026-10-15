using System;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    [Serializable]
    public sealed class CorrectionSmoothingConfig
    {
        [Min(0.01f)] public float DecaySeconds = 0.1f;
        [Min(0.01f)] public float SnapDistance = 3f;
    }
}
