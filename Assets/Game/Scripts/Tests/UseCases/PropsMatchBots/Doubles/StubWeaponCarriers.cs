using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.UseCases.PropsMatchBots.Doubles
{
    public sealed class StubWeaponCarriers : IWeaponCarriers, IWeaponsInHands
    {
        public IViewableMap<IWeapon, ICharacter> ByWeapon { get; } = new ViewableMap<IWeapon, ICharacter>();

        IViewableMap<IWeapon, IReadonlyCharacter> IWeaponsInHands.ByWeapon => _inHands;

        private readonly ViewableMap<IWeapon, IReadonlyCharacter> _inHands = new();

        public void Carry(IWeapon weapon, ICharacter carrier)
        {
            ByWeapon.Add(weapon, carrier);
            _inHands.Add(weapon, carrier);
        }

        public void Release(IWeapon weapon)
        {
            _inHands.Remove(weapon);
            ByWeapon.Remove(weapon);
        }
    }
}
