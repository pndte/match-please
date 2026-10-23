using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.View.Adapters
{
    public sealed class CharacterViews : ICharacterViews
    {
        public IViewableMap<IReadonlyCharacter, ICharacterView> ByCharacter { get; } = new ViewableMap<IReadonlyCharacter, ICharacterView>();
    }
}
