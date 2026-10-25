using Bw.UseCases.Combat.Abstractions.Characters;
using UnityEngine;

namespace Bw.UseCases.Combat.View.Adapters.Extensions
{
    public static class CharacterViewsExtensions
    {
        public static Vector2 ShownOffset(this ICharacterViews views, IReadonlyCharacter character) =>
            views.ByCharacter[character].Position - character.Hands.Pose.Position;
    }
}
