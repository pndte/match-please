using System;
using UnityEngine;

namespace Bw.UseCases.Props
{
    [Serializable]
    public sealed class HeldPropConfig
    {
        [Min(1f)] public float TurnSpeed = 900f;
        [Min(0f)] public float Gap = 0.05f;
        [Min(0f)] public float FeetClearance = 0.1f;
        public LayerMask Layer = 1 << 17;
    }
}
