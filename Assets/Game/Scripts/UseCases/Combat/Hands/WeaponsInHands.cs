using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Hands
{
    public sealed class WeaponsInHands : IWeaponsInHands
    {
        public IViewableMap<IWeapon, IReadonlyCharacter> ByWeapon { get; } = new ViewableMap<IWeapon, IReadonlyCharacter>();
    }
}
