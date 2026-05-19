using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using JetBrains.Collections.Viewable;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;

namespace Bw.Entities
{
    /// <summary>
    /// Fork of ViewableList with IReadonlyViewableList interface implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BwViewableList<T> : IViewableList<T>, IReadonlyViewableList<T> where T : notnull
    {
        [PublicAPI] public ISource<ListEvent<T>> Change => _myChange;

        private readonly IList<T> _myStorage;
        private readonly Signal<ListEvent<T>> _myChange = new();
     
        public BwViewableList(IList<T> list)
        {
            _myStorage = list;
        }
        
        public BwViewableList() : this(new List<T>())
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _myStorage.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            Add(item!);
        }

        public void Add([DisallowNull] T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _myStorage.Add(item);
            _myChange.Fire(ListEvent<T>.Add(_myStorage.Count - 1, item));
        }

        public void Clear()
        {
            for (int index = _myStorage.Count - 1; index >= 0; index--)
                RemoveAt(index);
        }

        public bool Contains(T item)
        {
            return _myStorage.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _myStorage.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var index = _myStorage.IndexOf(item);
            if (index < 0) return false;

            RemoveAt(index);
            return true;
        }

        public int Count => _myStorage.Count;

        public bool IsReadOnly => _myStorage.IsReadOnly;

        public void RemoveAt(int index)
        {
            var old = _myStorage[index];
            _myStorage.RemoveAt(index);
            _myChange.Fire(ListEvent<T>.Remove(index, old));
        }


        public T this[int index]
        {
            get => _myStorage[index];
            set
            {
                Assertion.Require(value != null, "value != null");

                var oldValue = _myStorage[index];
                if (EqualityComparer<T>.Default.Equals(oldValue, value)) return;

                _myStorage[index] = value;
                _myChange.Fire(ListEvent<T>.Update(index, oldValue, value));
            }
        }

        public void Advise(Lifetime lifetime, Action<ListEvent<T>> handler)
        {
            for (int index = 0; index < _myStorage.Count; index++)
            {
                try
                {
                    handler(ListEvent<T>.Add(index, _myStorage[index]));
                }
                catch (Exception e)
                {
                    Log.Root.Error(e);
                }
            }

            _myChange.Advise(lifetime, handler);
        }

        public int IndexOf(T item)
        {
            return _myStorage.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _myStorage.Insert(index, item);
            _myChange.Fire(ListEvent<T>.Add(index, item));
        }
    }
}