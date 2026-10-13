using System;
using System.Collections.Generic;
using Bw.Entities.Infrastructure;
using Bw.UseCases.Mortality;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class WeaponInventories : IWeaponInventories
    {
        public IViewableMap<IWeapon, IReadonlyMortal> Carriers => _carriers;
        public IViewableBiMap<IReadonlyMortal, IWeapon> Drawn { get; }

        private readonly ViewableMap<IWeapon, IReadonlyMortal> _carriers = new();
        private readonly Dictionary<IReadonlyMortal, IWeaponInventory> _inventories = new();
        private readonly IReadonlyWeaponKindRegistry _kindRegistry;
        private readonly WeaponInventoryConfig _config;

        public WeaponInventories(Lifetime lifetime, IReadonlyWeaponKindRegistry kindRegistry, WeaponInventoryConfig config)
        {
            Drawn = new ViewableBiMap<IReadonlyMortal, IWeapon>(lifetime);
            _kindRegistry = kindRegistry;
            _config = config;
        }

        public IWeaponInventory InventoryOf(IReadonlyMortal character)
        {
            if (!_inventories.TryGetValue(character, out var inventory))
                throw new InvalidOperationException("The character has no open inventory: one is opened for each life.");

            return inventory;
        }

        public void Open(Lifetime lifetime, IReadonlyMortal character)
        {
            if (_inventories.ContainsKey(character))
                throw new InvalidOperationException("A character has one inventory at a time.");

            var history = new DrawHistory(lifetime, character, Drawn, _carriers);
            _inventories.Add(character, new WeaponInventory(lifetime, character, _kindRegistry, _config, _carriers, Drawn, history));
            lifetime.OnTermination(() => _inventories.Remove(character));
        }
    }
}
