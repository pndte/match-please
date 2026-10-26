using System;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Combat.Abstractions.Inventory
{
    [Serializable]
    public struct WeaponKindRules
    {
        public WeaponKind Kind;
        public bool Droppable;
        public bool HandsOnly;
        public bool PressOnly;
    }
}
