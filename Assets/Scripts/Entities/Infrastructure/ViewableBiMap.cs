#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Infrastructure
{
    public interface IViewableBiMap<TLeft, TRight> : IViewableMap<TLeft, TRight>
        where TLeft : notnull
        where TRight : notnull
    {
        IViewableMap<TRight, TLeft> Inverse { get; }

        bool TryGetLeft(TRight right, out TLeft left);
        bool TryGetRight(TLeft left, out TRight right);

        bool RemoveRight(TRight right);

        void RebindLeft(TLeft left, TRight right);
        void RebindRight(TRight right, TLeft left);
    }

    /// <summary>
    /// Single-threaded (Unity main thread) viewable bimap.
    /// Source of truth: _forward.
    /// Reverse map is kept in sync via _forward.View(ownerLifetime, ...).
    /// </summary>
    public sealed class ViewableBiMap<TLeft, TRight> : IViewableBiMap<TLeft, TRight>
        where TLeft : notnull
        where TRight : notnull
    {
        public IViewableMap<TRight, TLeft> Inverse => _inverseView;
        public ISource<MapEvent<TLeft, TRight>> Change => _forward.Change;
        public TRight this[TLeft key]
        {
            get => _forward[key];
            set => RebindLeft(key, value);
        }

        public ICollection<TLeft> Keys => _forward.Keys;
        public ICollection<TRight> Values => _forward.Values;
        public int Count => _forward.Count;
        public bool IsReadOnly => ((IDictionary<TLeft, TRight>)_forward).IsReadOnly;
        
        private readonly ViewableMap<TLeft, TRight> _forward = new();
        private readonly ViewableMap<TRight, TLeft> _inverseStorage = new();
        private readonly InverseView _inverseView;

        public ViewableBiMap(Lifetime ownerLifetime)
        {
            _inverseView = new InverseView(this);

            _forward.View(ownerLifetime, (itemLifetime, left, right) =>
            {
                _inverseStorage.Add(right, left);
                itemLifetime.OnTermination(() =>
                {
                    _inverseStorage.Remove(right);
                });
            });
        }
        
        public void Advise(Lifetime lifetime, Action<MapEvent<TLeft, TRight>> handler)
            => _forward.Change.Advise(lifetime, handler);

        public void View(Lifetime lifetime, Action<Lifetime, TLeft, TRight> action)
            => _forward.View(lifetime, action);

        public bool TryGetLeft(TRight right, out TLeft left) => _inverseStorage.TryGetValue(right, out left);
        public bool TryGetRight(TLeft left, out TRight right) => _forward.TryGetValue(left, out right);

        public bool ContainsKey(TLeft key) => _forward.ContainsKey(key);
        public bool TryGetValue(TLeft key, out TRight value) => _forward.TryGetValue(key, out value);
        
        public bool Contains(KeyValuePair<TLeft, TRight> item)
            => ((ICollection<KeyValuePair<TLeft, TRight>>)_forward).Contains(item);

        public void CopyTo(KeyValuePair<TLeft, TRight>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<TLeft, TRight>>)_forward).CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<TLeft, TRight>> GetEnumerator() => _forward.GetEnumerator();

        public void Add(TLeft left, TRight right)
        {
            if (_forward.ContainsKey(left))
                throw new ArgumentException($"Left key already exists: {left}", nameof(left));

            if (_inverseStorage.ContainsKey(right))
                throw new ArgumentException($"Right key already exists: {right}", nameof(right));

            _forward.Add(left, right);
        }

        public bool Remove(TLeft left) => _forward.Remove(left);

        public bool RemoveRight(TRight right)
        {
            if (!_inverseStorage.TryGetValue(right, out var left))
                return false;

            return _forward.Remove(left);
        }

        public void Clear() => _forward.Clear();

        public void RebindLeft(TLeft left, TRight right)
        {
            if (_inverseStorage.TryGetValue(right, out var existingLeft) &&
                !EqualityComparer<TLeft>.Default.Equals(existingLeft, left))
            {
                throw new InvalidOperationException(
                    $"Right '{right}' already bound to '{existingLeft}'.");
            }

            if (_forward.TryGetValue(left, out var oldRight))
            {
                if (EqualityComparer<TRight>.Default.Equals(oldRight, right))
                    return;

                _forward.Remove(left);
            }

            _forward.Add(left, right);
        }

        public void RebindRight(TRight right, TLeft left)
        {
            if (_forward.TryGetValue(left, out var existingRight) &&
                !EqualityComparer<TRight>.Default.Equals(existingRight, right))
            {
                throw new InvalidOperationException(
                    $"Left '{left}' already bound to '{existingRight}'.");
            }

            if (_inverseStorage.TryGetValue(right, out var oldLeft))
            {
                if (EqualityComparer<TLeft>.Default.Equals(oldLeft, left))
                    return;

                _forward.Remove(oldLeft);
            }

            _forward.Add(left, right);
        }

        public void Add(KeyValuePair<TLeft, TRight> item) => Add(item.Key, item.Value);

        public bool Remove(KeyValuePair<TLeft, TRight> item)
        {
            if (_forward.TryGetValue(item.Key, out var v) &&
                EqualityComparer<TRight>.Default.Equals(v, item.Value))
                return _forward.Remove(item.Key);

            return false;
        }

        void IDictionary<TLeft, TRight>.Add(TLeft key, TRight value) => Add(key, value);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private sealed class InverseView : IViewableMap<TRight, TLeft>
        {
            private readonly ViewableBiMap<TLeft, TRight> _owner;
            public InverseView(ViewableBiMap<TLeft, TRight> owner) => _owner = owner;

            public ISource<MapEvent<TRight, TLeft>> Change => _owner._inverseStorage.Change;

            public void Advise(Lifetime lifetime, Action<MapEvent<TRight, TLeft>> handler)
                => _owner._inverseStorage.Change.Advise(lifetime, handler);

            public void View(Lifetime lifetime, Action<Lifetime, TRight, TLeft> action)
                => _owner._inverseStorage.View(lifetime, action);

            public bool ContainsKey(TRight key) => _owner._inverseStorage.ContainsKey(key);
            public bool TryGetValue(TRight key, out TLeft value) => _owner._inverseStorage.TryGetValue(key, out value);

            public TLeft this[TRight key]
            {
                get => _owner._inverseStorage[key];
                set => _owner.RebindRight(key, value);
            }

            public ICollection<TRight> Keys => _owner._inverseStorage.Keys;
            public ICollection<TLeft> Values => _owner._inverseStorage.Values;

            public int Count => _owner._inverseStorage.Count;
            public bool IsReadOnly => false;

            public void Add(TRight key, TLeft value) => _owner.Add(value, key);
            public bool Remove(TRight key) => _owner.RemoveRight(key);
            public void Clear() => _owner.Clear();

            void IDictionary<TRight, TLeft>.Add(TRight key, TLeft value) => Add(key, value);
            public void Add(KeyValuePair<TRight, TLeft> item) => Add(item.Key, item.Value);

            public bool Contains(KeyValuePair<TRight, TLeft> item)
                => ((ICollection<KeyValuePair<TRight, TLeft>>)_owner._inverseStorage).Contains(item);

            public void CopyTo(KeyValuePair<TRight, TLeft>[] array, int arrayIndex)
                => ((ICollection<KeyValuePair<TRight, TLeft>>)_owner._inverseStorage).CopyTo(array, arrayIndex);

            public bool Remove(KeyValuePair<TRight, TLeft> item)
            {
                if (_owner._inverseStorage.TryGetValue(item.Key, out var v) &&
                    EqualityComparer<TLeft>.Default.Equals(v, item.Value))
                    return _owner.RemoveRight(item.Key);

                return false;
            }

            public IEnumerator<KeyValuePair<TRight, TLeft>> GetEnumerator()
                => _owner._inverseStorage.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}