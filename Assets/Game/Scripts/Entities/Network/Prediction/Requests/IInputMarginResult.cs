using Bw.Entities.Network.Variables;

namespace Bw.Entities.Network.Prediction.Requests
{
    public interface IInputMarginResult
    {
        public INetSignal<int> Received { get; }
    }
}
