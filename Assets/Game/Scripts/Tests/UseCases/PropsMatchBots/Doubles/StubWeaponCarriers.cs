using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;

namespace Bw.Tests.UseCases.PropsMatchBots.Doubles
{
    public sealed class StubWeaponCarriers : IWeaponCarriers, IWeaponsInHands
    {
        public IReadonlyViewableMap<IWeapon, ICharacter> ByWeapon => _carried;

        IReadonlyViewableMap<IWeapon, IReadonlyCharacter> IReadonlyWeaponsInHands.ByWeapon => _inHands;

        private readonly BwViewableMap<IWeapon, ICharacter> _carried = new();
        private readonly BwViewableMap<IWeapon, IReadonlyCharacter> _inHands = new();

        public void Add(Lifetime carriedLifetime, IWeapon weapon, ICharacter carrier) =>
            _carried.AddLifetimed(carriedLifetime, weapon, carrier);

        public void Add(Lifetime heldLifetime, IWeapon weapon, IReadonlyCharacter holder) =>
            _inHands.AddLifetimed(heldLifetime, weapon, holder);

        public void Carry(IWeapon weapon, ICharacter carrier)
        {
            _carried.Add(weapon, carrier);
            _inHands.Add(weapon, carrier);
        }

        public void Release(IWeapon weapon)
        {
            _inHands.Remove(weapon);
            _carried.Remove(weapon);
        }
    }
}
