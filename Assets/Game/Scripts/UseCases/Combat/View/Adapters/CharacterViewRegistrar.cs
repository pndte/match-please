using Bw.Entities.Extensions;
using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Adapters
{
    public sealed class CharacterViewRegistrar
    {
        public CharacterViewRegistrar(
            Lifetime lifetime,
            IReadonlyCharacter character,
            ICharacterView view,
            ICharacterViews views)
        {
            views.ByCharacter.AddLifetimed(lifetime, character, view);
        }
    }
}
