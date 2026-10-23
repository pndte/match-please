using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Variables;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class TickedStateForm<TState, TWire> : IWireForm<TickedState<TState>, TickedState<TWire>>
        where TState : struct
        where TWire : struct
    {
        private readonly IWireForm<TState, TWire> _form;

        public TickedStateForm(IWireForm<TState, TWire> form)
        {
            _form = form;
        }

        public TickedState<TWire> WireOf(TickedState<TState> value) =>
            new(value.Tick, _form.WireOf(value.State));

        public TickedState<TState> ValueOf(TickedState<TWire> wire) =>
            new(wire.Tick, _form.ValueOf(wire.State));
    }
}
