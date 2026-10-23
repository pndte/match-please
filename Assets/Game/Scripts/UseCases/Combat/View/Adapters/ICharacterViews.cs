using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.View.Adapters
{
    public interface ICharacterViews
    {
        public IViewableMap<IReadonlyCharacter, ICharacterView> ByCharacter { get; }
    }
}
