using System;
using UnityEngine;

namespace Bw.UseCases.Combat.Shooting
{
    [Serializable]
    public sealed class PelletsConfig
    {
        [Min(1)] public int Count = 1;
        [Min(0f)] public float Cone;
    }
}
