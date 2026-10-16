using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Network;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Injection.Weapon
{
    public sealed class HeldWeaponParts<TState> where TState : struct
    {
        public Transform Weapon { get; }
        public Transform Visual { get; }
        public NetworkObject NetworkObject { get; }
        public NetworkWeaponHold Hold { get; }
        public TState Initial { get; }
        public WeaponKind Kind { get; }

        public HeldWeaponParts(
            Transform weapon,
            Transform visual,
            NetworkObject networkObject,
            NetworkWeaponHold hold,
            TState initial,
            WeaponKind kind)
        {
            Weapon = weapon;
            Visual = visual;
            NetworkObject = networkObject;
            Hold = hold;
            Initial = initial;
            Kind = kind;
        }
    }
}
