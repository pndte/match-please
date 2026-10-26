using Bw.Entities;
using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Characters
{
    public sealed class ControlledCharacterRegistrar
    {
        public ControlledCharacterRegistrar(
            Lifetime lifetime,
            IReadonlyCharacter character,
            IReadonlyControlledBy controlledBy,
            IControlledCharacters controlled)
        {
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime => controlled.Add(controlledLifetime, character));
        }
    }
}
