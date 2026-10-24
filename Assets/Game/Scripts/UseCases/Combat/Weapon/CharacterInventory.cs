using System;
using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Extensions;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon
{
    public sealed class CharacterInventory : IWeaponInventory, IRestorable<InventoryContent>
    {
        public IReadOnlyList<WeaponSlot> Slots => _config.Slots;
        public IReadonlyViewableMap<IWeapon, WeaponSlot> Carried => _carried;
        public IReadonlyProperty<InventoryContent> State => _content;

        private readonly BwViewableMap<IWeapon, WeaponSlot> _carried = new();
        private readonly ViewableProperty<InventoryContent> _content = new(InventoryContent.Empty);
        private readonly Dictionary<IWeapon, Carry> _carries = new();
        private readonly List<IWeapon> _unlisted = new();
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
            _carried.Advise(lifetime, _ => _content.Value = ContentOf(_carried));
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

        public void Apply(InventoryContent content)
        {
            if (!_life.IsAlive && content.Items.Count > 0)
                throw new InvalidOperationException("A dead character carries nothing: its inventory takes a content only while it lives.");

            _unlisted.Clear();
            foreach (var (weapon, slot) in _carried)
                if (!content.IsListed(weapon, slot))
                    _unlisted.Add(weapon);

            foreach (var weapon in _unlisted)
                if (!RulesOf(weapon).Droppable)
                    throw new InvalidOperationException($"The content leaves out an undroppable {RulesOf(weapon).Kind.ToString()} weapon, and such a weapon stays in its slot while the character lives.");

            foreach (var weapon in _unlisted)
                Drop(weapon);

            for (var index = 0; index < content.Items.Count; index++)
                Take(content.Items[index]);
        }

        private void Take(InventoryItem item)
        {
            if (_carried.TryGetValue(item.Weapon, out var slot) && slot == item.Slot)
                return;

            if (_carriers.ByWeapon.TryGetValue(item.Weapon, out var carrier))
                carrier.Inventory.Drop(item.Weapon);

            Give(item.Slot, item.Weapon);
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

        private static InventoryContent ContentOf(IReadonlyViewableMap<IWeapon, WeaponSlot> carried)
        {
            var items = new InventoryItem[carried.Count];
            var index = 0;
            foreach (var (weapon, slot) in carried)
                items[index++] = new InventoryItem(weapon, slot);

            return new InventoryContent(items);
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
