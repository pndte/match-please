using System;
using UnityEngine;

namespace Bw.UseCases.Props
{
    [Serializable]
    public sealed class HeldPropConfig
    {
        [Min(1f)] public float TurnSpeed = 480f;
        [Min(0f)] public float Gap = 0.05f;
        public LayerMask Layer = 1 << 17;
    }
}
