using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class InputSteps<TInput> where TInput : struct
    {
        private readonly IStepInputs<TInput> _inputs;
        private readonly ISteppable<TInput> _simulation;

        public InputSteps(
            Lifetime lifetime,
            IWorldStep worldStep,
            IStepInputs<TInput> inputs,
            ISteppable<TInput> simulation)
        {
            _inputs = inputs;
            _simulation = simulation;

            worldStep.Stepped(TickPhase.Default).Advise(lifetime, Step);
        }

        private void Step(int tick)
        {
            if (_inputs.TryTakeInputAt(tick, out var input))
                _simulation.Step(input);
        }
    }
}
