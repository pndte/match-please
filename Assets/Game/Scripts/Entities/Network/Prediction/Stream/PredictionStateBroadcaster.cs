using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
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
            IPredictionStateResult<TState> stateResult)
        {
            simulation.Stepped.Advise(lifetime, state =>
                stateResult.Received.Fire(new TickedState<TState>(ticks.Current, state)));
        }
    }
}
