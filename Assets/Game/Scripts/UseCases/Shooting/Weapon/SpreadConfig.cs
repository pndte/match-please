using System;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
{
    [Serializable]
    public sealed class SpreadConfig
    {
        public float Climb;
        [Min(0f)] public float Jitter;
    }
}
