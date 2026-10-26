using System;
using UnityEngine;

namespace Bw.UseCases.Combat.View.Impact
{
    [Serializable]
    public sealed class ShotImpactConfig
    {
        public GameObject Prefab;
        public GameObject Blocked;
        public LayerMask BodyLayers = 1 << 15;
    }
}
