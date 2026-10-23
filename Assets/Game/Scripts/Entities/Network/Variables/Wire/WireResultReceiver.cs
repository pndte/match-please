using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Variables
{
    public sealed class WireResultReceiver<TValue, TWire> : INetResultReceiver<TValue>
    {
        public ISource<TValue> Received => _received;

        private readonly Signal<TValue> _received = new();

        public WireResultReceiver(Lifetime lifetime, INetResultReceiver<TWire> receiver, IWireForm<TValue, TWire> form)
        {
            receiver.Received.Advise(lifetime, wire => _received.Fire(form.ValueOf(wire)));
        }
    }
}
