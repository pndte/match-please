using System;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class AmmoExtensions
    {
        public static bool Empty(this IReadonlyAmmo ammo)
        {
            return ammo.Value == 0;
        }

        public static bool Full(this IReadonlyAmmo ammo) =>
            ammo.Value >= ammo.Max;       
        
        public static void WhenEmpty(this IReadonlyAmmo ammo, Lifetime lifetime, Action<Lifetime> handler)
        {
            ammo.View(lifetime, (lf, bulletCount) =>
            {
                if (bulletCount == 0)
                    handler(lf);
            });
        }
    }
}