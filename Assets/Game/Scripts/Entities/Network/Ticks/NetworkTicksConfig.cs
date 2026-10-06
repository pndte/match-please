using System;
using UnityEngine;

namespace Bw.Entities.Network.Ticks
{
    [Serializable]
    public sealed class NetworkTicksConfig
    {
        [Header("Input lead")]
        [Min(0f)] public float InitialLeadTicks = 2f;
        [Min(0f)] public float MinLeadTicks = 0f;
        [Min(0f)] public float MaxLeadTicks = 15f;
        [Min(0f)] public float TargetMarginTicks = 1f;
        [Min(0f)] public float LeadRaiseRate = 0.5f;
        [Min(0f)] public float LeadLowerRate = 0.25f;
        [Min(1)] public int MarginReportIntervalTicks = 15;

        [Header("Input buffer")]
        [Min(1)] public int MaxBufferedInputTicks = 32;

        [Header("Interpolation")]
        [Min(0)] public int InterpolationDelayTicks = 1;

        [Header("Clock")]
        [Min(1)] public int MaxTicksPerFrame = 5;
        [Min(0f)] public float HardResetSeconds = 0.25f;
        [Min(0f)] public float MaxTimeScaleCorrection = 0.05f;
    }
}
