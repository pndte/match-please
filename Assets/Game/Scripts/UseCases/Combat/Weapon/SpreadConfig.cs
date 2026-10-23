using System;
using UnityEngine;

namespace Bw.UseCases.Combat.Weapon
{
    [Serializable]
    public sealed class SpreadConfig
    {
        public float Climb;
        [Min(0f)] public float Jitter;
        [Min(0f)] public float Return;
    }
}
