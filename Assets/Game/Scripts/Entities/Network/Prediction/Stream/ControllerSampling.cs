using System;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class ControllerSampling<TInput> where TInput : struct
    {
        private readonly IControllerRegistry _controllerRegistry;
        private readonly IControllerSamplers<TInput> _samplers;
        private readonly ISteppable<TInput> _simulation;

        public ControllerSampling(
            Lifetime lifetime,
            IControlledBy controlledBy,
            IControllerRegistry controllerRegistry,
            IControllerSamplers<TInput> samplers,
            INetworkTicks ticks,
            ISteppable<TInput> simulation)
        {
            _controllerRegistry = controllerRegistry;
            _samplers = samplers;
            _simulation = simulation;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                ticks.Ticked(TickPhase.Default).Advise(controlledLifetime, Sample));
        }

        private void Sample(int tick) =>
            _simulation.Step(SamplerOfController().Sample());

        private IInputSampler<TInput> SamplerOfController()
        {
            if (_controllerRegistry.Users.Count != 1)
                throw new InvalidOperationException("An object controlled on the server takes the input of its one controller, and it has several.");

            var controller = _controllerRegistry.Users[0];
            if (!_samplers.TryGetValue(controller, out var sampler))
                throw new InvalidOperationException(
                    $"Player '{controller.Nickname}' controls the object on the server and has no {typeof(TInput).Name} sampler: a player the server drives registers its samplers while it controls anything.");

            return sampler;
        }
    }
}
