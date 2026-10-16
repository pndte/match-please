using Bw.Entities.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class HeldWeaponBody
    {
        public HeldWeaponBody(Lifetime lifetime, IWeaponHold hold, PhysicsBody body)
        {
            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
            {
                body.enabled = false;
                heldLifetime.OnTermination(() =>
                {
                    if (body.isValid)
                        body.enabled = true;
                });
            });
        }
    }
}
