using System;
using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Entities.Infrastructure;
using Bw.UseCases.Character;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class WeaponInventories : IWeaponInventories
    {
        public IViewableMap<IWeapon, IReadonlyCharacter> Carriers => _carriers;
        public IViewableBiMap<IReadonlyCharacter, IWeapon> Drawn { get; }

        private readonly ViewableMap<IWeapon, IReadonlyCharacter> _carriers = new();
        private readonly Dictionary<IWeapon, Carry> _carries = new();

        public WeaponInventories(Lifetime lifetime)
        {
            Drawn = new ViewableBiMap<IReadonlyCharacter, IWeapon>(lifetime);
        }

        public void Give(Lifetime lifetime, IReadonlyCharacter character, WeaponSlot slot, IWeapon weapon)
        {
            if (_carries.ContainsKey(weapon))
                throw new InvalidOperationException("A weapon is carried by one character at a time.");

            if (TryFind(character, slot, out _))
                throw new InvalidOperationException($"The character's {slot.ToString()} slot is taken: its weapon has to leave the inventory first.");

            var carry = lifetime.CreateNested();
            _carries.Add(weapon, new Carry(character, slot, carry));
            carry.Lifetime.OnTermination(() => _carries.Remove(weapon));
            _carriers.AddLifetimed(carry.Lifetime, weapon, character);
        }

        public void Draw(IWeapon weapon)
        {
            var carry = CarryOf(weapon);
            if (Drawn.TryGetRight(carry.Character, out var drawn) && drawn == weapon)
                return;

            Drawn.Remove(carry.Character);
            Drawn.AddLifetimed(carry.Definition.Lifetime, carry.Character, weapon);
        }

        public void Drop(IWeapon weapon)
        {
            var carry = CarryOf(weapon);
            if (carry.Slot == WeaponSlot.Fists)
                throw new InvalidOperationException("Fists are never dropped: they stay in their slot while the character lives.");

            var drawn = Drawn.TryGetRight(carry.Character, out var current) && current == weapon;
            carry.Definition.Terminate();

            if (drawn && TryFind(carry.Character, WeaponSlot.Fists, out var fists))
                Draw(fists);
        }

        public void PickUp(Lifetime lifetime, IReadonlyCharacter character, IWeapon weapon)
        {
            if (_carries.ContainsKey(weapon))
                throw new InvalidOperationException("Only a weapon nobody carries can be picked up.");

            if (TryFind(character, WeaponSlot.Primary, out var carried))
                _carries[carried].Definition.Terminate();

            Give(lifetime, character, WeaponSlot.Primary, weapon);
            Draw(weapon);
        }

        private Carry CarryOf(IWeapon weapon)
        {
            if (!_carries.TryGetValue(weapon, out var carry))
                throw new InvalidOperationException("The weapon is in no inventory: only a carried weapon can be drawn or dropped.");

            return carry;
        }

        private bool TryFind(IReadonlyCharacter character, WeaponSlot slot, out IWeapon weapon)
        {
            foreach (var (carried, carry) in _carries)
            {
                if (carry.Character != character || carry.Slot != slot)
                    continue;

                weapon = carried;
                return true;
            }

            weapon = null;
            return false;
        }

        private readonly struct Carry
        {
            public readonly IReadonlyCharacter Character;
            public readonly WeaponSlot Slot;
            public readonly LifetimeDefinition Definition;

            public Carry(IReadonlyCharacter character, WeaponSlot slot, LifetimeDefinition definition)
            {
                Character = character;
                Slot = slot;
                Definition = definition;
            }
        }
    }
}
