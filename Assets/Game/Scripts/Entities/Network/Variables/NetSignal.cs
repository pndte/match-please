using System;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Variables
{
    public interface INetSignal<T> : INetSyncEntry, ISignal<T>
    {
        public void FireTo(IClient recipient, T value);
        
        internal T PendingPayload { get; }
        internal NetSendTarget PendingTarget { get; }
        internal void ApplyFromNetwork(T value);
    }

    public sealed class NetSignal<T> : INetSignal<T>
    {
        public IScheduler Scheduler
        {
            get => _inner.Scheduler;
            set => _inner.Scheduler = value;
        }

        T INetSignal<T>.PendingPayload => _pending;
        NetSendTarget INetSignal<T>.PendingTarget => _pendingTarget;
        IViewableProperty<bool> INetSyncEntry.Dirty => _dirty;

        private readonly Signal<T> _inner = new();
        private readonly ViewableProperty<bool> _dirty = new(false);
        private readonly INetSendGuard _sendGuard;
        private T _pending;
        private NetSendTarget _pendingTarget = NetSendTarget.Untargeted;

        internal NetSignal(INetSendGuard sendGuard)
        {
            _sendGuard = sendGuard;
        }

        public void Advise(Lifetime lifetime, Action<T> handler)
        {
            _inner.Advise(lifetime, handler);
        }

        public void Fire(T value) =>
            Send(NetSendTarget.Untargeted, value);

        public void FireTo(IClient recipient, T value) =>
            Send(NetSendTarget.To(recipient), value);

        void INetSignal<T>.ApplyFromNetwork(T value)
        {
            _inner.Fire(value);
        }

        void INetSyncEntry.Accept(INetSyncVisitor visitor)
        {
            visitor.VisitSignal(this);
        }

        private void Send(NetSendTarget target, T value)
        {
            _sendGuard.Check(target);

            _pending = value;
            _pendingTarget = target;
            _inner.Fire(value);
            _dirty.Value = true;
        }
    }
}
