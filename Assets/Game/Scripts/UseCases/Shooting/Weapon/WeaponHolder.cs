using Bw.Entities;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Shooting.Weapon
{
    public class WeaponHolder : MonoBehaviour, IHolder<IWeapon>
    {
        public IWeapon Value { get; private set; }
        public IOwnershipController OwnershipController { get; private set; }
        public IControlledBy ControlledBy { get; private set; }

        [Inject]
        private void Construct(
            Lifetime lifetime, 
            IWeapon weapon, 
            IOwnershipController ownershipController,
            IControlledBy controlledBy)
        {
            Value = weapon;
            OwnershipController = ownershipController;
            ControlledBy = controlledBy;
        }

        public void PickUpWeapon(Lifetime lifetime, CharacterHolder characterHolder) // TODO: это должен делать сервер в отдельном сервисе
        {
            transform.SetParent(characterHolder.transform);
            transform.localPosition = Vector3.right;
            var physics = GetComponent<Rigidbody2D>();
            physics.simulated = false;

            lifetime.OnTermination(() =>
            {
                transform.SetParent(null, worldPositionStays: true);
                physics.simulated = true;
                physics.AddForce(Vector2.up * 2f, ForceMode2D.Impulse);
            });
        }
    }
}