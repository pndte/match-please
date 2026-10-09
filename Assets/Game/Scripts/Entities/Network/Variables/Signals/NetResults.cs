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
        public ISource<T> Received => FromNetwork;

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
    }
}
