using System;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Impact
{
    [Serializable]
    public sealed class StrikeImpactConfig
    {
        public GameObject Body;
        public GameObject Surface;
        public LayerMask BodyLayers = 1 << 15;
    }
}
