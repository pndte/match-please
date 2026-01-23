using System;
using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Extensions
{
    public static class ViewablePropertyExtensions
    {
        public static void WhenLessOrEquals<T>(this IReadonlyProperty<T> property, Lifetime lifetime, T value, Action handler) where T:  struct, IComparable<T>
        {
            property.Advise(lifetime, comparable =>
            {
                if (comparable.CompareTo(value) <= 0) handler();
            });
        }

        public static void AdviseAdd<K, V>(this IViewableMap<K, V> map, Lifetime lifetime, Action<K, V> handler)
        {
            map.AdviseAddRemove(lifetime, (addRemove, key, value) =>
            {
                if (addRemove == AddRemove.Add) handler(key, value);
            });
        }
        
        public static void AddLifetimed<K, V>(this IViewableMap<K, V> map, Lifetime lifetime, K key, V value)
        {
            map.AddLifetimed(lifetime, new KeyValuePair<K, V>(key, value));
        }
    }
}