using Bw.Entities.Players;
using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Prediction.Stream
{
    public interface IControllerSamplers<TInput> : IViewableMap<IPlayer, IInputSampler<TInput>> where TInput : struct
    {
    }

    public sealed class ControllerSamplers<TInput> : ViewableMap<IPlayer, IInputSampler<TInput>>, IControllerSamplers<TInput> where TInput : struct
    {
    }
}
