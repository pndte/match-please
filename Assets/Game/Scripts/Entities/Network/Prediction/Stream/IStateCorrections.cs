using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Prediction.Stream
{
    public interface IStateCorrections<TState> where TState : struct
    {
        public ISource<StateCorrection<TState>> Corrected { get; }
    }
}
