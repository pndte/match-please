using System;
using Bw.UseCases.Audio.View.Playback;
using Bw.UseCases.Shooting.Weapon;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Audio
{
    [Serializable]
    public sealed class WeaponSoundsConfig
    {
        public ShootingWeaponConfig Weapon;
        public Sound Shot = new();
        public ReloadCue[] Reload = Array.Empty<ReloadCue>();
    }

    [Serializable]
    public sealed class ReloadCue
    {
        public Sound Sound = new();
        [Range(0f, 1f)] public float At;
        public bool OnlyEmpty;
    }
}
