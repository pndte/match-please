using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class InputRelay<TInput> where TInput : struct
    {
        private readonly List<IClient> _clients = new();
        private readonly List<TickedInput<TInput>> _heard = new();
        private readonly INetResultSender<TickedInput<TInput>> _relayed;
        private readonly IInputAudience _audience;

        public InputRelay(
            Lifetime lifetime,
            DriverInputs<TInput> inputs,
            IWorldStep worldStep,
            INetResultSender<TickedInput<TInput>> relayed,
            IInputAudience audience)
        {
            _relayed = relayed;
            _audience = audience;

            inputs.Heard.Advise(lifetime, _heard.Add);
            worldStep.Stepped(TickPhase.Publish).Advise(lifetime, _ => Relay());
        }

        private void Relay()
        {
            if (_heard.Count == 0)
                return;

            _clients.Clear();
            _audience.Collect(_clients);
            for (var input = 0; input < _heard.Count; input++)
                for (var client = 0; client < _clients.Count; client++)
                    _relayed.SendTo(_clients[client], _heard[input]);

            _heard.Clear();
        }
    }
}
