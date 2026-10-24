using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class InputMarginClientHandler
    {
        public InputMarginClientHandler(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            INetResultReceiver<int> margin,
            IInputMarginFeedback marginFeedback)
        {
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                margin.Received.Advise(controlledLifetime, marginFeedback.Report));
        }
    }
}
