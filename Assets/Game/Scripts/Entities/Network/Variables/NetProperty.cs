using System;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Variables
{
    public interface INetProperty<T> : IViewableProperty<T>, INetSyncEntry
    {
        internal void ApplyFromNetwork(T value);
    }

    public class NetProperty<T> : INetProperty<T>
    {
        public ISource<T> Change => _inner.Change;
        public Maybe<T> Maybe => _inner.Maybe;

        public T Value
        {
            get => _inner.Value;
            set
            {
                _sendGuard.Check(NetSendTarget.Untargeted);

                _inner.Value = value;
                _dirty.Value = true;
            }
        }

        IViewableProperty<bool> INetSyncEntry.Dirty => _dirty;

        private readonly ViewableProperty<T> _inner;
        private readonly ViewableProperty<bool> _dirty = new(false);
        private readonly INetSendGuard _sendGuard;

        internal NetProperty(T initial, INetSendGuard sendGuard)
        {
            _inner = new ViewableProperty<T>(initial);
            _sendGuard = sendGuard;
        }

        public void Advise(Lifetime lifetime, Action<T> handler)
        {
            _inner.Advise(lifetime, handler);
        }

        void INetProperty<T>.ApplyFromNetwork(T value)
        {
            _inner.Value = value;
        }

        void INetSyncEntry.Accept(INetSyncVisitor visitor)
        {
            visitor.VisitProperty(this);
        }
    }
}
