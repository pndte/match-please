using System;
using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Extensions;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon
{
    public sealed class CharacterInventory : IWeaponInventory
    {
        public IReadOnlyList<WeaponSlot> Slots => _config.Slots;
        public IReadonlyViewableMap<IWeapon, WeaponSlot> Carried => _carried;

        private readonly BwViewableMap<IWeapon, WeaponSlot> _carried = new();
        private readonly Dictionary<IWeapon, Carry> _carries = new();
        private readonly IWeaponCarriers _carriers;
        private readonly IReadonlyWeaponKindRegistry _kinds;
        private readonly WeaponInventoryConfig _config;

        private Lifetime _life = Lifetime.Terminated;

        public CharacterInventory(
            Lifetime lifetime,
            IReadonlyMortal mortal,
            IWeaponCarriers carriers,
            IReadonlyWeaponKindRegistry kinds,
            WeaponInventoryConfig config)
        {
            _carriers = carriers;
            _kinds = kinds;
            _config = config;

            mortal.State.WhenAlive(lifetime, Live);
        }

        public WeaponKindRules RulesOf(IWeapon weapon) =>
            _carries.TryGetValue(weapon, out var carry) ? carry.Rules : _config.RulesOf(_kinds.KindOf(weapon));

        public void Give(WeaponSlot slot, IWeapon weapon)
        {
            if (!_life.IsAlive)
                throw new InvalidOperationException("A dead character carries nothing: its inventory opens with each life.");

            if (_carriers.ByWeapon.ContainsKey(weapon))
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

            var carry = _life.CreateNested();
            _carries.Add(weapon, new Carry(rules, carry));
            _carried.AddLifetimed(carry.Lifetime, weapon, slot);
            carry.Lifetime.OnTermination(() => _carries.Remove(weapon));
        }

        public void Drop(IWeapon weapon)
        {
            var carry = CarryOf(weapon);
            if (!carry.Rules.Droppable)
                throw new InvalidOperationException($"{carry.Rules.Kind.ToString()} weapons are never dropped: they stay in their slot while the character lives.");

            carry.Definition.Terminate();
        }

        private void Live(Lifetime aliveLifetime)
        {
            _life = aliveLifetime;
            aliveLifetime.OnTermination(() => _life = Lifetime.Terminated);
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
