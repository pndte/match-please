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

        public static void View<T>(this IViewableList<T> list, Lifetime lifetime, Action<Lifetime, T> handler)
        {
            list.View(lifetime, (itemLifetime, _, item) =>
                handler(itemLifetime, item));
        }
        
        public static void AddLifetimed<K, V>(this IViewableMap<K, V> map, Lifetime lifetime, K key, V value)
        {
            map.AddLifetimed(lifetime, new KeyValuePair<K, V>(key, value));
        }

        public static void ViewForKey<K, V>(this IViewableMap<K, V> map, Lifetime lifetime, K targetKey,
            Action<Lifetime, V> handler)
        {
            LifetimeDefinition def = null;
            map.AdviseAddRemove(lifetime, (kind, newKey, value) =>
            {
                switch (kind)
                {
                    case AddRemove.Add:
                        if (Equals(newKey, targetKey))
                        {
                            def = Lifetime.Define(lifetime);
                            handler(def.Lifetime, value);
                        }

                        break;
                    case AddRemove.Remove:
                        if (Equals(newKey, targetKey))
                        {
                            def?.Terminate();
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Illegal enum value: {kind}");
                }
            });
        }
    }
}