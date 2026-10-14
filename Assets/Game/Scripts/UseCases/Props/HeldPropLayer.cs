using Bw.Entities.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Props
{
    public sealed class HeldPropLayer
    {
        public HeldPropLayer(
            Lifetime lifetime,
            IWeaponHold hold,
            GameObject prop,
            int heldLayer)
        {
            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
            {
                var freeLayer = prop.layer;
                prop.layer = heldLayer;
                heldLifetime.OnTermination(() =>
                {
                    if (prop.layer == heldLayer)
                        prop.layer = freeLayer;
                });
            });
        }
    }
}
