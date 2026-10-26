using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Hands
{
    public sealed class WeaponsInHands : IWeaponsInHands
    {
        public IReadonlyViewableMap<IWeapon, IReadonlyCharacter> ByWeapon => _byWeapon;

        private readonly BwViewableMap<IWeapon, IReadonlyCharacter> _byWeapon = new();

        public void Add(Lifetime heldLifetime, IWeapon weapon, IReadonlyCharacter holder) =>
            _byWeapon.AddLifetimed(heldLifetime, weapon, holder);
    }
}
