using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Lifetimes;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Mortality
{
    public sealed class CorpseHitbox
    {
        public CorpseHitbox(
            Lifetime lifetime,
            IReadonlyMortal mortal,
            PhysicsBody body,
            int corpseLayer)
        {
            mortal.State.WhenDead(lifetime, _ => body.MoveToLayer(corpseLayer));
        }
    }
}
