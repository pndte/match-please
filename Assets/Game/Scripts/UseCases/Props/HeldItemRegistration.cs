using System;
using Bw.Entities;
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
            var hands = HoldersHands();
            _grip.Attach(heldLifetime, hands.Body);
            hands.Take(heldLifetime, held);
        }

        private IHands HoldersHands()
        {
            var holder = _item.parent;
            if (holder == null || !holder.TryGetComponent<IHolder<IHands>>(out var hands))
                throw new InvalidOperationException($"'{_item.name}' can be held only by a holder with hands: its root needs a hands holder.");

            return hands.Value;
        }
    }
}
