using System;
using System.Collections.Generic;
using Bw.UseCases.Combat.Weapon.Pickup.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Pickup
{
    public sealed class FloorWeapons : IFloorWeapons
    {
        public IReadOnlyList<IFloorWeapon> Lying => _lying;

        private readonly List<IFloorWeapon> _lying = new();

        public void AddLifetimed(Lifetime lifetime, IFloorWeapon weapon)
        {
            if (_lying.Contains(weapon))
                throw new InvalidOperationException("A weapon lies on the floor once at a time: it was added again before its last stay there ended.");

            _lying.Add(weapon);
            lifetime.OnTermination(() => _lying.Remove(weapon));
        }
    }
}
