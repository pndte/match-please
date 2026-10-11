using System;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Highlight
{
    [Serializable]
    public sealed class WeaponHighlightConfig
    {
        public Material Material;
        [Min(0.01f)] public float FadeInTime = 0.12f;
        [Min(0.01f)] public float FadeOutTime = 0.2f;
    }
}
