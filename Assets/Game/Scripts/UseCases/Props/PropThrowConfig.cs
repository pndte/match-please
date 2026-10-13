using System;
using Bw.UseCases.Mortality;
using UnityEngine;

namespace Bw.UseCases.Props
{
    [Serializable]
    public sealed class PropThrowConfig : IHitConfig
    {
        [Min(0f)] public float Speed = 22f;
        [Min(0f)] public float Damage = 5f;
        [Min(0f)] public float Knockback = 8f;
        public LayerMask Targets = 1 << 15;

        float IHitConfig.Damage => Damage;
        float IHitConfig.Knockback => Knockback;
    }
}
