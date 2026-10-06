using System;
using Bw.UseCases.Audio.View.Playback;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Audio
{
    [Serializable]
    public sealed class WeaponSoundsConfig
    {
        [Header("Shot")]
        public Sound Shot = new();
        public Sound GroundHit = new();
        public LayerMask GroundLayers;

        [Header("Reload")]
        public Sound MagazineOut = new();
        public Sound MagazineIn = new();
        [Min(0f)] public float MagazineInBeforeEnd = 1.1f;
        public Sound BoltRack = new();
        [Min(0f)] public float BoltRackBeforeEnd = 0.6f;
    }
}
