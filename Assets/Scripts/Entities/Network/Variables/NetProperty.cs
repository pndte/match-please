using System;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Variables
{
    public interface INetProperty
    {
        public IViewableProperty<bool> Dirty { get; } 
        public void Accept(INetPropertyVisitor visitor);
    }

    public interface INetProperty<T> : IViewableProperty<T>, INetProperty
    {
    }

    public class NetProperty<T> : INetProperty<T> 
    {
        public ISource<T> Change => _inner.Change;
        public Maybe<T> Maybe => _inner.Maybe;
        public IViewableProperty<bool> Dirty { get; } = new ViewableProperty<bool>(false);

        T IViewableProperty<T>.Value
        {
            get => _inner.Value;
            set
            {
                _inner.Value = value;
                Dirty.Value = true;
            }
        }
        T IReadonlyProperty<T>.Value => _inner.Value;
        
        private readonly ViewableProperty<T> _inner;

        internal NetProperty(T initial)
        {
            _inner = new ViewableProperty<T>(initial);
        }
        
        public void Advise(Lifetime lifetime, Action<T> handler)
        {
            _inner.Advise(lifetime, handler);
        }
        
        void INetProperty.Accept(INetPropertyVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}