using System;
using Bw.UseCases.Audio.View.Playback;
using UnityEngine;

namespace Bw.UseCases.Movement.View.Audio
{
    [Serializable]
    public sealed class MovementSoundsConfig
    {
        public Sound Step = new();
        public Sound Jump = new();
        public Sound Land = new();

        [Header("Steps")]
        [Min(0.05f)] public float StepInterval = 0.4f;
        [Range(0f, 1f)] public float MinStepSpeed = 0.05f;
    }
}
