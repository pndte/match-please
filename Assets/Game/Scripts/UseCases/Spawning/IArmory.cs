using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Spawning
{
    public interface IArmory
    {
        public IWeapon CreateWeapon(
            Lifetime lifetime,
            WeaponDescription weapon,
            Vector3 position,
            Quaternion rotation);
    }
}
