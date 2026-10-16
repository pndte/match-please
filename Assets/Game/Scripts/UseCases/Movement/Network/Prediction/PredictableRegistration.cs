using Bw.Entities.Extensions;
using JetBrains.Lifetimes;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PredictableRegistration
    {
        public PredictableRegistration(
            Lifetime lifetime,
            PhysicsBody body,
            IPredictable predictable,
            IPredictableCollection predictables)
        {
            predictables.AddLifetimed(lifetime, body, predictable);
        }
    }
}
