using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities
{
    public sealed class BwViewableSet<T> : IReadonlyViewableSet<T>
    {
        public int Count => _set.Count;

        private readonly ViewableSet<T> _set = new();

        public void Advise(Lifetime lifetime, Action<SetEvent<T>> handler) =>
            _set.Advise(lifetime, handler);

        public bool Contains(T item) =>
            _set.Contains(item);

        public void Add(T item)
        {
            if (_set.Contains(item))
                throw new InvalidOperationException($"'{item}' is in the set already: a set holds every item once.");

            _set.Add(item);
        }

        public bool Remove(T item) =>
            _set.Remove(item);

        public IEnumerator<T> GetEnumerator() =>
            _set.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
