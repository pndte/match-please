using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Movement.Network.Prediction;
using UnityEngine;

namespace Bw.UseCases.Mortality.Network.Prediction
{
    public sealed class KnockbackRules : IPushRules<Impacts>
    {
        public Vector2 Impulse(Impacts impacts) =>
            impacts.Knockback();
    }
}
