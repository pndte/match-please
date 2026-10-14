using System;
using Bw.UseCases.Audio.View.Playback;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Audio
{
    [Serializable]
    public sealed class ReloadCue
    {
        public Sound Sound = new();
        [Range(0f, 1f)] public float At;
        public bool OnlyEmpty;
    }
}
