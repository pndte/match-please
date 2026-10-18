using Bw.Entities.Extensions;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Props
{
    public sealed class HeldItemRegistration
    {
        private readonly Transform _item;
        private readonly HeldPropGrip _grip;

        public HeldItemRegistration(
            Lifetime lifetime,
            IWeaponHold hold,
            Transform item,
            IHeldItem held,
            HeldPropGrip grip)
        {
            _item = item;
            _grip = grip;

            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime => Take(heldLifetime, held));
        }

        private void Take(Lifetime heldLifetime, IHeldItem held)
        {
            var hands = _item.HolderService<IHands>();
            _grip.Enter(heldLifetime);
            hands.Take(heldLifetime, held);
        }
    }
}
