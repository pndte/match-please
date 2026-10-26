using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon
{
    public sealed class WeaponCarriers : IWeaponCarriers
    {
        public IReadonlyViewableMap<IWeapon, ICharacter> ByWeapon => _byWeapon;

        private readonly BwViewableMap<IWeapon, ICharacter> _byWeapon = new();

        public void Add(Lifetime carriedLifetime, IWeapon weapon, ICharacter carrier) =>
            _byWeapon.AddLifetimed(carriedLifetime, weapon, carrier);
    }
}
