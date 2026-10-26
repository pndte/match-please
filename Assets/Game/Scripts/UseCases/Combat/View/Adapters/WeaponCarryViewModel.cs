using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Adapters
{
    public sealed class WeaponCarryViewModel : IWeaponCarryViewModel
    {
        public IReadonlyProperty<bool> Held => _held;
        public IReadonlyProperty<bool> Drawn => _drawn;
        public IReadonlyProperty<bool> Mine => _controlledBy.Me;
        public ICharacterView Holder => _carriers.ByWeapon.TryGetValue(_weapon, out var carrier)
            ? _views.ByCharacter[carrier]
            : throw new InvalidOperationException("Nobody carries the weapon, so it has no holder: a view asks for the holder only while the weapon is held.");

        private readonly ViewableProperty<bool> _held = new(false);
        private readonly ViewableProperty<bool> _drawn = new(false);
        private readonly IWeapon _weapon;
        private readonly IReadonlyWeaponCarriers _carriers;
        private readonly ICharacterViews _views;
        private readonly IReadonlyControlledBy _controlledBy;

        public WeaponCarryViewModel(
            Lifetime lifetime,
            IWeapon weapon,
            IReadonlyWeaponCarriers carriers,
            IReadonlyWeaponsInHands inHands,
            ICharacterViews views,
            IReadonlyControlledBy controlledBy)
        {
            _weapon = weapon;
            _carriers = carriers;
            _views = views;
            _controlledBy = controlledBy;

            carriers.ByWeapon.ViewForKey(lifetime, weapon, (carriedLifetime, _) => Raise(_held, carriedLifetime));
            inHands.ByWeapon.ViewForKey(lifetime, weapon, (drawnLifetime, _) => Raise(_drawn, drawnLifetime));
        }

        private static void Raise(ViewableProperty<bool> flag, Lifetime raisedLifetime)
        {
            flag.Value = true;
            raisedLifetime.OnTermination(() => flag.Value = false);
        }
    }
}
