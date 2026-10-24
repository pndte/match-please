using Bw.Entities;
using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Characters
{
    public sealed class ControlledCharacters : BwViewableList<IReadonlyCharacter>, IControlledCharacters
    {
        public void Add(Lifetime lifetime, IReadonlyCharacter character) =>
            this.AddLifetimed(lifetime, character);
    }
}
