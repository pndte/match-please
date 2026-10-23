using Bw.Entities.Network.Objects;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Injection.Weapon
{
    public sealed class HeldWeaponParts<TState> where TState : struct
    {
        public Transform Weapon { get; }
        public Transform Visual { get; }
        public NetworkObject NetworkObject { get; }
        public INetworkLifetimedObject NetworkLifetimed { get; }
        public TState Initial { get; }
        public WeaponKind Kind { get; }

        public HeldWeaponParts(
            Transform weapon,
            Transform visual,
            NetworkObject networkObject,
            INetworkLifetimedObject networkLifetimed,
            TState initial,
            WeaponKind kind)
        {
            Weapon = weapon;
            Visual = visual;
            NetworkObject = networkObject;
            NetworkLifetimed = networkLifetimed;
            Initial = initial;
            Kind = kind;
        }
    }
}
