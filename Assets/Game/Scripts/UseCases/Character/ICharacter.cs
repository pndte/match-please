using Bw.Entities;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Character
{
    public interface ICharacter : IReadonlyCharacter, IAppliedState<CharacterVitals>
    {
    }

    public interface IReadonlyCharacter : IReadonlyAppliedState<CharacterVitals>
    {
        public IReadonlyHealth Health { get; }
        public IReadonlyProperty<CharacterState> State { get; }
    }
}
