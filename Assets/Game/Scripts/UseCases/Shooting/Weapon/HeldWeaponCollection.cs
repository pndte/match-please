using Bw.Entities.Infrastructure;
using Bw.UseCases.Character;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class HeldWeaponCollection : IHeldWeaponCollection
    {
        public IViewableBiMap<ICharacter, IWeapon> ByCharacter { get; }

        public HeldWeaponCollection(Lifetime lifetime)
        {
            ByCharacter = new ViewableBiMap<ICharacter, IWeapon>(lifetime);
        }
    }
}
