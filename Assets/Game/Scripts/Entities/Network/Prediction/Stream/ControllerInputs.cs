using System;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class ControllerInputs<TInput> where TInput : struct
    {
        public ISource<TInput> Simulated => _simulated;

        private readonly Signal<TInput> _simulated = new();
        private readonly IControllerRegistry _controllerRegistry;
        private readonly IControllerSamplers<TInput> _samplers;

        public ControllerInputs(
            Lifetime lifetime,
            PredictionInputBuffer<TInput> buffer,
            IControlledBy controlledBy,
            IControllerRegistry controllerRegistry,
            IControllerSamplers<TInput> samplers,
            INetworkTicks ticks)
        {
            _controllerRegistry = controllerRegistry;
            _samplers = samplers;

            controlledBy.Me.View(lifetime, (controlLifetime, controlledHere) =>
            {
                if (controlledHere)
                    ticks.Ticked(TickPhase.Default).Advise(controlLifetime, Sample);
                else
                    buffer.Simulated.Advise(controlLifetime, _simulated.Fire);
            });
        }

        private void Sample(int tick) =>
            _simulated.Fire(SamplerOfController().Sample());

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
