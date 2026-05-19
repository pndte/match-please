using System;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Variables
{
    public interface INetSignal<T> : INetSyncEntry, ISignal<T>
    {
        T PendingPayload { get; }
        void ApplyFromNetwork(T value);
    }
    
    public sealed class NetSignal<T> : INetSignal<T>
    {
        private readonly Signal<T> _inner = new();
        private T _pending;
        public IViewableProperty<bool> Dirty { get; } = new ViewableProperty<bool>(false);

        public T PendingPayload => _pending;
        public IScheduler Scheduler
        {
            get => _inner.Scheduler;
            set => _inner.Scheduler = value;
        }

        public void Advise(Lifetime lifetime, Action<T> handler)
        {
            _inner.Advise(lifetime, handler);
        }

        public void Fire(T value)
        {
            _pending = value;
            _inner.Fire(value);
            Dirty.Value = true;
        }

        void INetSignal<T>.ApplyFromNetwork(T value)
        {
            _inner.Fire(value);
        }

        void INetSyncEntry.Accept(INetSyncVisitor visitor)
        {
            visitor.VisitSignal(this);
        }
    }
}
