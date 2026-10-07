using System;
using UnityEngine;

namespace Bw.UseCases.Character
{
    [Serializable]
    public sealed class CharacterDeathConfig
    {
        [Min(0)] public float CorpseTime = 3f;
        public LayerMask CorpseLayer = 1 << 2;
    }
}
