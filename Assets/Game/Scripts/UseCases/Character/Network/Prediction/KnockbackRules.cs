using Bw.UseCases.Movement.Network.Prediction;
using UnityEngine;

namespace Bw.UseCases.Character.Network.Prediction
{
    public sealed class KnockbackRules : IPushRules<Hit>
    {
        public Vector2 Impulse(Hit hit) =>
            hit.Knockback;
    }
}
