using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Prediction.Stream
{
    public interface IPredictedBodyCollection : IViewableList<IPredictedBody>
    {
    }

    public sealed class PredictedBodyCollection : ViewableList<IPredictedBody>, IPredictedBodyCollection
    {
    }
}
