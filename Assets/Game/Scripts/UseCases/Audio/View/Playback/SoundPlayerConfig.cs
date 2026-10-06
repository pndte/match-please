using System;
using UnityEngine;

namespace Bw.UseCases.Audio.View.Playback
{
    [Serializable]
    public sealed class SoundPlayerConfig
    {
        [Min(1)] public int Voices = 32;

        [Header("Space")]
        [Range(0f, 1f)] public float SpatialBlend = 1f;
        [Range(0f, 360f)] public float Spread = 60f;
        [Min(0f)] public float MinDistance = 12f;
        [Min(0f)] public float MaxDistance = 45f;
    }
}
