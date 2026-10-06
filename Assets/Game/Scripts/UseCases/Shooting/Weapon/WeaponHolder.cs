using Bw.Entities;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Shooting.Weapon
{
    public class WeaponHolder : MonoBehaviour, IHolder<IWeapon>
    {
        public IWeapon Value { get; private set; }

        [Inject]
        private void Construct(IWeapon weapon)
        {
            Value = weapon;
        }
    }
}
