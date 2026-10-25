using System;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    [Serializable]
    public sealed class CorrectionSmoothingConfig
    {
        [Min(0.01f)] public float DecaySeconds = 0.2f;
        [Min(0.01f)] public float SnapDistance = 3f;
        [Range(0f, 180f)] public float SnapAngle = 45f;
        [Min(0.01f)] public float SwitchBlendSeconds = 0.15f;
    }
}
