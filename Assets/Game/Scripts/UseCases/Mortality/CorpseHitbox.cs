using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Mortality
{
    public sealed class CorpseHitbox
    {
        public CorpseHitbox(
            Lifetime lifetime,
            IReadonlyMortal mortal,
            BoxCollider2D hitbox,
            PhysicsBody body,
            int corpseLayer)
        {
            mortal.State.WhenDead(lifetime, _ =>
            {
                hitbox.gameObject.layer = corpseLayer;
                body.MoveToLayer(corpseLayer);
            });
        }
    }
}
