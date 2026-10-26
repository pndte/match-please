using System;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using Unity.Netcode;

namespace Bw.CoreAdapters.Weapons
{
    [Serializable]
    public struct WeaponEntry
    {
        public WeaponDescription Weapon;
        public NetworkObject Prefab;
        public WeaponKind Kind;
    }
}
