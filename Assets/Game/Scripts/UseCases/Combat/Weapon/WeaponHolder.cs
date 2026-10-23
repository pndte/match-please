using Bw.Entities;
using Bw.UseCases.Combat.Abstractions.Weapons;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Combat.Weapon
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
