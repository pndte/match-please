using System;
using System.Collections.Generic;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon
{
    public sealed class WeaponKindRegistry : IWeaponKindRegistry
    {
        private readonly Dictionary<IWeapon, WeaponKind> _kinds = new();

        public bool Contains(IWeapon weapon) =>
            _kinds.ContainsKey(weapon);

        public WeaponKind KindOf(IWeapon weapon)
        {
            if (!_kinds.TryGetValue(weapon, out var kind))
                throw new InvalidOperationException("The weapon has no kind: whoever creates a weapon or a prop registers its kind while it exists.");

            return kind;
        }

        public void AddLifetimed(Lifetime lifetime, IWeapon weapon, WeaponKind kind)
        {
            if (!_kinds.TryAdd(weapon, kind))
                throw new InvalidOperationException("A weapon has one kind at a time: it was registered again before its last registration ended.");

            lifetime.OnTermination(() => _kinds.Remove(weapon));
        }
    }
}
