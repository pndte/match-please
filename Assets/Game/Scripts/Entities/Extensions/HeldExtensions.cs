using System;
using UnityEngine;

namespace Bw.Entities.Extensions
{
    public static class HeldExtensions
    {
        public static bool IsHeld(this Transform item) =>
            item.parent != null;

        public static Transform Holder(this Transform held)
        {
            if (!held.IsHeld())
                throw new InvalidOperationException($"'{held.name}' is held by nobody: it has no holder.");

            return held.parent;
        }

        public static T HolderComponent<T>(this Transform held) where T : Component
        {
            var holder = held.Holder();
            if (!holder.TryGetComponent<T>(out var component))
                throw Missing(held, holder, typeof(T));

            return component;
        }

        public static T HolderService<T>(this Transform held)
        {
            var holder = held.Holder();
            if (!holder.TryGetComponent<IHolder<T>>(out var service))
                throw Missing(held, holder, typeof(T));

            return service.Value;
        }

        private static InvalidOperationException Missing(Transform held, Transform holder, Type part) =>
            new($"'{holder.name}' holds '{held.name}' but has no {part.Name}.");
    }
}
