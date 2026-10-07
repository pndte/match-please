using Bw.Entities;
using JetBrains.Collections.Viewable;
using JetBrains.Core;

namespace Bw.UseCases.Character
{
    public interface ICharacter : IReadonlyCharacter
    {
        public void Apply(CharacterVitals vitals);
    }

    public interface IReadonlyCharacter
    {
        public IReadonlyHealth Health { get; }
        public IReadonlyProperty<CharacterState> State { get; }
        public ISource<Unit> Died { get; }
        public ISource<CharacterVitals> Applied { get; }
    }
}
