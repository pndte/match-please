using Bw.Entities.Network.Rollback;
using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class CharacterPredictions : ICharacterPredictions
    {
        public IViewableMap<IReadonlyCharacter, IPredictedBy> ByCharacter { get; } = new ViewableMap<IReadonlyCharacter, IPredictedBy>();
    }
}
