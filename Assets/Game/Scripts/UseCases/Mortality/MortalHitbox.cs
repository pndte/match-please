using Bw.Entities;
using Bw.UseCases.Mortality.Extensions;
using Unity.Collections;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Mortality
{
    public sealed class MortalHitbox
    {
        public MortalHitbox(PhysicsBody body, IHolder<IReadonlyMortal> mortal)
        {
            using var shapes = body.GetShapes(Allocator.Temp);
            for (var index = 0; index < shapes.Length; index++)
                if (shapes[index].contactFilter.categories.bitMask != 0)
                    shapes[index].SetMortal(mortal);
        }
    }
}
