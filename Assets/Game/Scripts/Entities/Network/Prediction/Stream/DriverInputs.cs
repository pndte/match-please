using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class DriverInputs<TInput> : IStepInputs<TInput> where TInput : struct
    {
        public ISource<TickedInput<TInput>> Heard => _heard;

        private readonly Signal<TickedInput<TInput>> _heard = new();
        private readonly IControllerRegistry _controllers;
        private readonly IControllerSamplers<TInput> _samplers;
        private readonly PredictionInputBuffer<TInput> _buffer;

        private IPlayer _controller;
        private bool _drivenByServer;
        private TInput _previousInput;

        public DriverInputs(
            Lifetime lifetime,
            IControllerRegistry controllers,
            IReadonlyPlayerDrivers drivers,
            IControllerSamplers<TInput> samplers,
            PredictionInputBuffer<TInput> buffer)
        {
            _controllers = controllers;
            _samplers = samplers;
            _buffer = buffer;

            controllers.Users.View(lifetime, (userLifetime, controller) =>
            {
                buffer.Clear();
                drivers.Of(controller).View(userLifetime, (drivenLifetime, driver) =>
                    driver.Switch(_ => buffer.Stored.Advise(drivenLifetime, _heard.Fire), () => DriveByServer(drivenLifetime, controller)));
            });
        }

        public bool TryTakeInputAt(int tick, out TInput input)
        {
            if (_controllers.Users.Count == 0)
                input = default;
            else if (_drivenByServer)
                input = SamplerOf(_controller).Sample();
            else
                return _buffer.TryTakeInputAt(tick, out input);

            _heard.Fire(new TickedInput<TInput>(tick, input, _previousInput));
            _previousInput = input;
            return true;
        }

        private void DriveByServer(Lifetime drivenLifetime, IPlayer controller)
        {
            if (_drivenByServer)
                throw new InvalidOperationException("An object driven by the server takes the input of its one controller, and a second one came.");

            _controller = controller;
            _drivenByServer = true;
            drivenLifetime.OnTermination(() => _drivenByServer = false);
        }

        private IInputSampler<TInput> SamplerOf(IPlayer controller)
        {
            if (!_samplers.TryGetValue(controller, out var sampler))
                throw new InvalidOperationException(
                    $"Player '{controller.Nickname}' drives the object on the server and has no {typeof(TInput).Name} sampler: a player the server drives registers its samplers while it controls anything.");

            return sampler;
        }
    }
}
