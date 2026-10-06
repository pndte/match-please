using System;
using UnityEngine;

namespace Bw.Entities.Network.LagCompensation
{
    [Serializable]
    public sealed class LagCompensationConfig
    {
        [Min(0f)] public float MaxRewindSeconds = 0.4f;
    }
}
