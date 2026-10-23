using System;
using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Abstractions.Inventory.Extensions
{
    public static class WeaponCarriersExtensions
    {
        public static void ViewUncarried(
            this IWeaponCarriers carriers,
            Lifetime lifetime,
            IWeapon weapon,
            Action<Lifetime> handler)
        {
            var uncarried = new SequentialLifetimes(lifetime);
            carriers.ByWeapon.ViewForKey(lifetime, weapon, (carriedLifetime, _) =>
            {
                uncarried.TerminateCurrent();
                carriedLifetime.OnTermination(() =>
                {
                    if (lifetime.IsAlive)
                        handler(uncarried.Next());
                });
            });

            if (!carriers.ByWeapon.ContainsKey(weapon))
                handler(uncarried.Next());
        }
    }
}
