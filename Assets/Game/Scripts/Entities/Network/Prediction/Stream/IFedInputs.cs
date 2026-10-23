using Bw.Entities.Network.Prediction.Stream.Requests;
using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Prediction.Stream
{
    public interface IFedInputs<TInput> where TInput : struct
    {
        public ISource<TickedInput<TInput>> Fed { get; }
    }
}
