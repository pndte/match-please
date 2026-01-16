using System;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Entities
{
    public static class ViewablePropertyExtensions
    {
        public static void WhenLessOrEquals<T>(this IReadonlyProperty<T> property, Lifetime lifetime, T value, Action handler) where T:  struct, IComparable<T>, IConvertible
        {
            property.Advise(lifetime, comparable =>
            {
                if (comparable.CompareTo(value) <= 0) handler();
            });
        }
    }
}