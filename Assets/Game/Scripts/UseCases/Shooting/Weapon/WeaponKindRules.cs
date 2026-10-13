using System;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Shooting.Weapon
{
    [Serializable]
    public struct WeaponKindRules
    {
        public WeaponKind Kind;
        public bool Droppable;
        public bool HandsOnly;
    }
}
