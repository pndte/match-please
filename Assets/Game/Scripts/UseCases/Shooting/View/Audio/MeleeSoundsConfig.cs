using System;
using Bw.UseCases.Audio.View.Playback;
using Bw.UseCases.Shooting.Melee;

namespace Bw.UseCases.Shooting.View.Audio
{
    [Serializable]
    public sealed class MeleeSoundsConfig
    {
        public MeleeWeaponConfig Weapon;
        public Sound Swing = new();
        public Sound BodyHit = new();
        public Sound SurfaceHit = new();
    }
}
