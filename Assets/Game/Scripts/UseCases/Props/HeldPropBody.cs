using Bw.Entities.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Props
{
    public sealed class HeldPropBody
    {
        public HeldPropBody(Lifetime lifetime, IWeaponHold hold, PhysicsBody body)
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
