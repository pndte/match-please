using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Combat.Weapon
{
    public sealed class HeldWeaponBody
    {
        public HeldWeaponBody(
            Lifetime lifetime,
            IWeapon weapon,
            IWeaponCarriers carriers,
            PhysicsBody body)
        {
            carriers.ByWeapon.ViewForKey(lifetime, weapon, (carriedLifetime, _) =>
            {
                body.enabled = false;
                carriedLifetime.OnTermination(() =>
                {
                    if (body.isValid)
                        body.enabled = true;
                });
            });
        }
    }
}
