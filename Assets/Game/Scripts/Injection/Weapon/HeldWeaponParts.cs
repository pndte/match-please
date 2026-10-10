using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Network;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Bw.Injection.Weapon
{
    public sealed class HeldWeaponParts
    {
        public Transform Weapon { get; }
        public Transform Visual { get; }
        public NetworkObject NetworkObject { get; }
        public NetworkWeaponHold Hold { get; }
        public NetworkTransform NetworkTransform { get; }
        public Rigidbody2D Body { get; }
        public WeaponState Initial { get; }
        public KeyCode DrawKey { get; }

        public HeldWeaponParts(
            Transform weapon,
            Transform visual,
            NetworkObject networkObject,
            NetworkWeaponHold hold,
            NetworkTransform networkTransform,
            Rigidbody2D body,
            WeaponState initial,
            KeyCode drawKey)
        {
            Weapon = weapon;
            Visual = visual;
            NetworkObject = networkObject;
            Hold = hold;
            NetworkTransform = networkTransform;
            Body = body;
            Initial = initial;
            DrawKey = drawKey;
        }
    }
}
