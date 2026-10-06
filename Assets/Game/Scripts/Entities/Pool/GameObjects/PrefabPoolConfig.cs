using System;
using Bw.Entities.Pool.GameObjects.Modes;
using Bw.Entities.Utilities;
using UnityEngine;

namespace Bw.Entities.Pool.GameObjects
{
    [Serializable]
    public sealed class PrefabPoolConfig
    {
        public GameObject Prefab;
        [Min(0)] public int Prewarm = 4;
        [Min(0)] public int Limit = 8;
        [SerializeReference, TypePicker] public IPoolCapMode Mode = new ReclaimOldestMode();
    }
}
