using Bw.UseCases.Mortality.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Mortality
{
    public sealed class CorpseHitbox
    {
        public CorpseHitbox(Lifetime lifetime, IReadonlyMortal mortal, BoxCollider2D hitbox, int corpseLayer)
        {
            mortal.State.WhenDead(lifetime, _ => hitbox.gameObject.layer = corpseLayer);
        }
    }
}
