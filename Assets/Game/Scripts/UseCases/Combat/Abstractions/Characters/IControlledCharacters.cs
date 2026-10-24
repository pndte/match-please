using Bw.Entities;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Abstractions.Characters
{
    public interface IControlledCharacters : IReadonlyViewableList<IReadonlyCharacter>
    {
        public void Add(Lifetime lifetime, IReadonlyCharacter character);
    }
}
