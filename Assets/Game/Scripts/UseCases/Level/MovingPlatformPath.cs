using System;
using UnityEngine;

namespace Bw.UseCases.Level
{
    [Serializable]
    public sealed class MovingPlatformPath
    {
        public Vector2 Travel;
        [Min(0.01f)] public float TravelSeconds = 5f;
        [Min(0f)] public float PauseSeconds = 1f;
    }
}
