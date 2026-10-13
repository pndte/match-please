using System;
using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Entities.Infrastructure;
using Bw.UseCases.Mortality;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class WeaponInventory : IWeaponInventory
    {
        public IReadOnlyList<WeaponSlot> Slots => _config.Slots;
        public IReadOnlyDictionary<IWeapon, WeaponSlot> Carried => _carried;
        public IReadOnlyList<IWeapon> DrawOrder => _history.Order;

        private readonly Dictionary<IWeapon, WeaponSlot> _carried = new();
        private readonly Dictionary<IWeapon, Carry> _carries = new();
        private readonly Lifetime _lifetime;
        private readonly IReadonlyMortal _character;
        private readonly IReadonlyWeaponKindRegistry _kindRegistry;
        private readonly WeaponInventoryConfig _config;
        private readonly IViewableMap<IWeapon, IReadonlyMortal> _carriers;
        private readonly IViewableBiMap<IReadonlyMortal, IWeapon> _drawn;
        private readonly DrawHistory _history;

        public WeaponInventory(
            Lifetime lifetime,
            IReadonlyMortal character,
            IReadonlyWeaponKindRegistry kindRegistry,
            WeaponInventoryConfig config,
            IViewableMap<IWeapon, IReadonlyMortal> carriers,
            IViewableBiMap<IReadonlyMortal, IWeapon> drawn,
            DrawHistory history)
        {
            _lifetime = lifetime;
            _character = character;
            _kindRegistry = kindRegistry;
            _config = config;
            _carriers = carriers;
            _drawn = drawn;
            _history = history;
        }

        public WeaponKindRules RulesOf(IWeapon weapon) =>
            _carries.TryGetValue(weapon, out var carry) ? carry.Rules : _config.RulesOf(_kindRegistry.KindOf(weapon));

        public bool TryFindDrawn(out IWeapon drawn) =>
            _drawn.TryGetRight(_character, out drawn);

        public void Give(WeaponSlot slot, IWeapon weapon)
        {
            if (_carriers.ContainsKey(weapon))
                throw new InvalidOperationException("A weapon is carried by one character at a time.");

            if (Array.IndexOf(_config.Slots, slot) < 0)
                throw new InvalidOperationException("The slot is not in this inventory's layout.");

            var rules = RulesOf(weapon);
            if (!slot.IsFor(rules.Kind))
                throw new InvalidOperationException($"The slot takes no {rules.Kind.ToString()} weapons.");

            if (this.TryFindIn(slot, rules.Droppable, out _))
                throw new InvalidOperationException(rules.Droppable
                    ? "The slot already holds a droppable weapon: it has to leave the inventory first."
                    : "The slot already holds an undroppable weapon: a slot holds one of each.");

            var carry = _lifetime.CreateNested();
            _carried.Add(weapon, slot);
            _carries.Add(weapon, new Carry(rules, carry));
            carry.Lifetime.OnTermination(() =>
            {
                _carried.Remove(weapon);
                _carries.Remove(weapon);
            });

            _carriers.AddLifetimed(carry.Lifetime, weapon, _character);
        }

        public void Draw(IWeapon weapon)
        {
            var carry = CarryOf(weapon);
            if (this.Covered(weapon))
                throw new InvalidOperationException("A covered weapon is never drawn: the weapon lying over it has to leave the slot first.");

            if (TryFindDrawn(out var drawn))
            {
                if (drawn == weapon)
                    throw new InvalidOperationException("The weapon is in hands already.");

                if (!drawn.Stowable)
                    throw new InvalidOperationException("The weapon in hands can't be put away right now.");

                if (CarryOf(drawn).Rules.HandsOnly)
                    throw new InvalidOperationException("A hands-only weapon is never put away: it has to be dropped before another weapon is drawn.");

                _drawn.Remove(_character);
            }

            _drawn.AddLifetimed(carry.Definition.Lifetime, _character, weapon);
        }

        public void Drop(IWeapon weapon)
        {
            var carry = CarryOf(weapon);
            if (!carry.Rules.Droppable)
                throw new InvalidOperationException($"{carry.Rules.Kind.ToString()} weapons are never dropped: they stay in their slot while the character lives.");

            carry.Definition.Terminate();
        }

        private Carry CarryOf(IWeapon weapon)
        {
            if (!_carries.TryGetValue(weapon, out var carry))
                throw new InvalidOperationException("The weapon is not in this inventory.");

            return carry;
        }

        private readonly struct Carry
        {
            public readonly WeaponKindRules Rules;
            public readonly LifetimeDefinition Definition;

            public Carry(WeaponKindRules rules, LifetimeDefinition definition)
            {
                Rules = rules;
                Definition = definition;
            }
        }
    }
}
