using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class PredictionStateBroadcaster<TState> where TState : struct
    {
        public PredictionStateBroadcaster(
            Lifetime lifetime,
            INetworkTicks ticks,
            IReadonlySimulation<TState> simulation,
            INetResultSender<TickedState<TState>> result)
        {
            simulation.Stepped.Advise(lifetime, state =>
                result.Broadcast(new TickedState<TState>(ticks.Current, state)));
        }
    }
}
