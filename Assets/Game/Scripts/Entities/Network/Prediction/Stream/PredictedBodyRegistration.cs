using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class PredictedBodyRegistration
    {
        public PredictedBodyRegistration(Lifetime lifetime, IPredictedBody body, IPredictedBodyCollection bodies)
        {
            bodies.AddLifetimed(lifetime, body);
        }
    }
}
