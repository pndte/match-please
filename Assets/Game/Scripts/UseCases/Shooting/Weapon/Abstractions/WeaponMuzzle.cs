using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public class WeaponMuzzle : IWeaponMuzzle
    {
        public Transform Transform { get; }

        public WeaponMuzzle(Transform transform)
        {
            Transform = transform;
        }
    }
}