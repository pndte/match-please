using System;
using Bw.UseCases.Combat.Weapon;
using UnityEngine;

namespace Bw.UseCases.Match.View.Adapters
{
    [Serializable]
    public struct WeaponIcon
    {
        public WeaponDescription Weapon;
        public Sprite Icon;
    }
}
