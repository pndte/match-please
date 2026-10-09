using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Variables
{
    internal abstract class NetSignal<T> : INetSignal<T>
    {
        T INetSignal<T>.PendingPayload => _pending;
        NetSendTarget INetSignal<T>.PendingTarget => _pendingTarget;
        IViewableProperty<bool> INetSyncEntry.Dirty => _dirty;

        private protected ISource<T> FromNetwork => _fromNetwork;

        private readonly Signal<T> _fromNetwork = new();
        private readonly ViewableProperty<bool> _dirty = new(false);
        private readonly INetSendGuard _sendGuard;
        private T _pending;
        private NetSendTarget _pendingTarget = NetSendTarget.Untargeted;

        private protected NetSignal(INetSendGuard sendGuard)
        {
            _sendGuard = sendGuard;
        }

        void INetSignal<T>.ApplyFromNetwork(T value) =>
            _fromNetwork.Fire(value);

        void INetSyncEntry.Accept(INetSyncVisitor visitor) =>
            visitor.VisitSignal(this);

        private protected void Stage(NetSendTarget target, T value)
        {
            _sendGuard.Check(target);

            _pending = value;
            _pendingTarget = target;
        }

        private protected void Dispatch() =>
            _dirty.Value = true;
    }
}
