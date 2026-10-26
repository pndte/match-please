using Bw.Entities;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Abstractions.Hands
{
    public interface IReadonlyWeaponsInHands
    {
        public IReadonlyViewableMap<IWeapon, IReadonlyCharacter> ByWeapon { get; }
    }

    public interface IWeaponsInHands : IReadonlyWeaponsInHands
    {
        public void Add(Lifetime heldLifetime, IWeapon weapon, IReadonlyCharacter holder);
    }
}
