using System;
using UnityEngine;

namespace Bw.UseCases.Mortality
{
    [Serializable]
    public sealed class DeathConfig
    {
        [Min(0)] public float CorpseTime = 3f;
        public LayerMask CorpseLayer = 1 << 2;
    }
}
