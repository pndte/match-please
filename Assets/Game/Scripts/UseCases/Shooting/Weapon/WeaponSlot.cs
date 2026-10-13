using System;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Shooting.Weapon
{
    [Serializable]
    public sealed class WeaponSlot
    {
        public WeaponKind[] Kinds = Array.Empty<WeaponKind>();
    }
}
