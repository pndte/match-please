using System;
using Bw.UseCases.Shooting.Melee;
using Bw.UseCases.Shooting.View.Audio;

namespace Bw.UseCases.Shooting.View.Adapters.Audio
{
    [Serializable]
    public sealed class MeleeSoundsConfig : MeleeSounds
    {
        public MeleeWeaponConfig Weapon;
    }
}
