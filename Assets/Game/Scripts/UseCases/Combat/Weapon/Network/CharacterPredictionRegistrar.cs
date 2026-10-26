using Bw.Entities.Extensions;
using Bw.Entities.Network.Rollback;
using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class CharacterPredictionRegistrar
    {
        public CharacterPredictionRegistrar(
            Lifetime lifetime,
            IReadonlyCharacter character,
            IPredictedBy predictedBy,
            ICharacterPredictions predictions)
        {
            predictions.ByCharacter.AddLifetimed(lifetime, character, predictedBy);
        }
    }
}
