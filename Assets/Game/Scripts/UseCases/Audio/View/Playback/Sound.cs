using System;
using UnityEngine;

namespace Bw.UseCases.Audio.View.Playback
{
    [Serializable]
    public sealed class Sound
    {
        public AudioClip[] Clips = Array.Empty<AudioClip>();
        [Range(0f, 1f)] public float Volume = 1f;
        [Range(0f, 0.5f)] public float PitchVariation = 0.05f;
    }
}
