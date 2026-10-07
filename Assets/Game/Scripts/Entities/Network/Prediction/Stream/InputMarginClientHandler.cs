using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class InputMarginClientHandler
    {
        public InputMarginClientHandler(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IInputMarginResult marginResult,
            IInputMarginFeedback marginFeedback)
        {
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                marginResult.Received.Advise(controlledLifetime, marginFeedback.Report));
        }
    }
}
