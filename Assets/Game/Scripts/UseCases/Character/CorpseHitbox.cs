using Bw.UseCases.Character.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character
{
    public sealed class CorpseHitbox
    {
        public CorpseHitbox(Lifetime lifetime, IReadonlyCharacter character, BoxCollider2D hitbox, int corpseLayer)
        {
            character.State.WhenDead(lifetime, _ => hitbox.gameObject.layer = corpseLayer);
        }
    }
}
