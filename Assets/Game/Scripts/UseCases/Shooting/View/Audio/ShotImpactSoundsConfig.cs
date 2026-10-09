using System;
using Bw.UseCases.Audio.View.Playback;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Audio
{
    [Serializable]
    public sealed class ShotImpactSoundsConfig
    {
        public Sound GroundHit = new();
        public LayerMask GroundLayers;
    }
}
