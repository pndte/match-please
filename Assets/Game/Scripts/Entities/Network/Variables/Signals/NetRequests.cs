using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Variables
{
    public interface INetRequestSender<T>
    {
        public ISource<T> Sent { get; }
        public void Send(T request);
    }

    public interface INetRequestReceiver<T>
    {
        public ISource<T> Received { get; }
    }

    internal sealed class NetRequest<T> : NetSignal<T>, INetRequestSender<T>, INetRequestReceiver<T>
    {
        public ISource<T> Sent => _sent;
        public ISource<T> Received => _received;
        public ISource<(ulong SenderClientId, T Request)> ReceivedFromClient => _receivedFromClient;

        private readonly Signal<T> _sent = new();
        private readonly Signal<T> _received = new();
        private readonly Signal<(ulong SenderClientId, T Request)> _receivedFromClient = new();

        internal NetRequest(INetSendGuard sendGuard) : base(sendGuard)
        {
        }

        public void Send(T request)
        {
            Stage(NetSendTarget.Untargeted, request);
            _sent.Fire(request);
            Dispatch();
        }

        private protected override void Heard(T value, ulong senderClientId)
        {
            _received.Fire(value);
            _receivedFromClient.Fire((senderClientId, value));
        }
    }
}
