using Bw.Entities.Network.Rollback;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class RemoteInputsRegistration<TInput> where TInput : struct
    {
        public RemoteInputsRegistration(
            Lifetime lifetime,
            IPredictedBy predictedBy,
            RemoteInputs<TInput> inputs,
            IRollbackParts parts)
        {
            predictedBy.Me.WhenTrue(lifetime, predictedLifetime => parts.Watch(predictedLifetime, inputs));
        }
    }
}
