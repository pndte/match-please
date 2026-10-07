using System;
using UnityEngine;

namespace Bw.UseCases.Character.View.Death
{
    [Serializable]
    public sealed class CharacterDeathViewConfig
    {
        public GameObject Blood;
        [Min(0f)] public float BurstTime = 0.3f;
        public Vector2 BurstOffset = new(0f, 0.1f);
    }
}
