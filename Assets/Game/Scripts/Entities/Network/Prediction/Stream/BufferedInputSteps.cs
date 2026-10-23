using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class BufferedInputSteps<TInput> where TInput : struct
    {
        public BufferedInputSteps(
            Lifetime lifetime,
            PredictionInputBuffer<TInput> buffer,
            IControlledBy controlledBy,
            ISteppable<TInput> simulation)
        {
            controlledBy.Me.WhenFalse(lifetime, remoteLifetime =>
                buffer.Simulated.Advise(remoteLifetime, simulation.Step));
        }
    }
}
