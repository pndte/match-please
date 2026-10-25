using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.Doubles
{
    public sealed class ManualResults<T> : INetResultReceiver<T>, INetRequestReceiver<T>
    {
        public ISource<T> Received => _received;

        private readonly Signal<T> _received = new();

        public void Deliver(T value) =>
            _received.Fire(value);
    }
}
