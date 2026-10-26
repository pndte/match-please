using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Variables
{
    public interface INetResultSender<T>
    {
        public void Broadcast(T result);
        public void SendTo(IClient recipient, T result);
    }

    public interface INetResultReceiver<T>
    {
        public ISource<T> Received { get; }
    }

    internal sealed class NetResult<T> : NetSignal<T>, INetResultSender<T>, INetResultReceiver<T>
    {
        public ISource<T> Received => _received;

        private readonly Signal<T> _received = new();

        internal NetResult(INetSendGuard sendGuard) : base(sendGuard)
        {
        }

        public void Broadcast(T result)
        {
            Stage(NetSendTarget.Untargeted, result);
            Dispatch();
        }

        public void SendTo(IClient recipient, T result)
        {
            Stage(NetSendTarget.To(recipient), result);
            Dispatch();
        }

        private protected override void Heard(T value, ulong senderClientId) =>
            _received.Fire(value);
    }
}
