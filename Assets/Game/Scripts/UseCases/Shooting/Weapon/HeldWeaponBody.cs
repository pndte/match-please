using Bw.Entities.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class HeldWeaponBody
    {
        private const float DropImpulse = 2f;

        public HeldWeaponBody(Lifetime lifetime, IWeaponHold hold, Rigidbody2D body)
        {
            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
            {
                body.simulated = false;
                heldLifetime.OnTermination(() =>
                {
                    body.simulated = true;
                    body.AddForce(Vector2.up * DropImpulse, ForceMode2D.Impulse);
                });
            });
        }
    }
}
