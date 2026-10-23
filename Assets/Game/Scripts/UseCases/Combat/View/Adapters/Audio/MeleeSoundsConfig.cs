using System;
using Bw.UseCases.Combat.Melee;
using Bw.UseCases.Combat.View.Audio;

namespace Bw.UseCases.Combat.View.Adapters.Audio
{
    [Serializable]
    public sealed class MeleeSoundsConfig : MeleeSounds
    {
        public MeleeWeaponConfig Weapon;
    }
}
